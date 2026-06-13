# Decisões Arquiteturais: Justificativas e Trade-offs

Este documento detalha as decisões arquiteturais tomadas durante o planejamento, suas justificativas e possíveis alternativas.

---

## 1. Gerenciamento de Estado: Singleton `ProjectService`

### ✅ Decisão Escolhida
Usar um **Singleton `ProjectService`** que gerencia todo o estado do projeto em memória.

### 🎯 Justificativa
- **Single Source of Truth:** Apenas uma instância `Project` em memória evita inconsistências
- **Acesso Global:** Qualquer Form pode acessar via `ProjectService.GetInstance()`
- **Estado Consistente:** Todos os Forms veem os mesmos dados
- **Simplez Implementação:** Padrão bem conhecido e fácil de entender

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: Dependency Injection (DI Container)
```csharp
// Cada Form recebe ProjectService injetado
public class MainForm
{
    private readonly ProjectService _projectService;
    public MainForm(ProjectService projectService)
    {
        _projectService = projectService;
    }
}
```
**Problemas:**
- Mais complexo configurar (precisa registrar tipos no container)
- Overhead para projeto pequeno/médio
- WinForms não tem suporte nativo a DI

#### ❌ Alternativa 2: Static Properties (Mais Simples que Singleton)
```csharp
public class ProjectService
{
    public static Project CurrentProject { get; set; }
}
```
**Problemas:**
- Menos testável (difícil mockar statics)
- Sem controle sobre inicialização
- Singleton oferece mais controle

#### ❌ Alternativa 3: Event Bus / Observer Pattern
```csharp
// Cada Form se inscreve em eventos de mudança de estado
projectService.ProjectChanged += MainForm_OnProjectChanged;
```
**Problemas:**
- Mais complexo gerenciar inscrições
- Difícil debugar fluxo de dados
- Overhead desnecessário para projeto monolítico

### ✅ Por Que Singleton é Melhor
- Balanceamento perfeito entre **simplicidade** e **controle**
- Adequado para aplicação desktop monolítica
- Fácil de entender e manter
- Permite testes com mock: `ProjectService._instance = mockService;`

---

## 2. Persistência: Newtonsoft.Json + Custom Converters

### ✅ Decisão Escolhida
Usar **Newtonsoft.Json (JSON.NET)** com **Custom Converters** para tipos complexos.

### 🎯 Justificativa
- **Robust:** Newtonsoft.Json é padrão de facto em .NET (40M+ downloads/semana)
- **Flexível:** Custom converters permitem serializar tipos complexos (Guid, Listas, referências)
- **Human-Readable:** JSON é legível (usuário pode inspecionar arquivo)
- **Performance:** JSON é mais rápido que XML ou banco de dados
- **Local Storage:** Sem dependência de servidor/conexão

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: System.Text.Json (Nativo)
```csharp
// .NET 8+ inclui System.Text.Json
var options = new JsonSerializerOptions();
JsonSerializer.Serialize(project, options);
```
**Problemas:**
- Converters customizados mais verbosos
- Menos maduro que Newtonsoft (até .NET 8)
- Comunidade ainda prefere Newtonsoft.Json

#### ❌ Alternativa 2: XML
```csharp
var xmlDoc = new XmlDocument();
xmlDoc.LoadXml(xmlString);
```
**Problemas:**
- Muito verboso e pesado
- Performance ruim para estruturas aninhadas
- Difícil de ler manualmente

#### ❌ Alternativa 3: SQLite Local
```csharp
// Embutido no projeto, sem servidor
using (var connection = new SqliteConnection("Data Source=project.db"))
{
    // ...
}
```
**Problemas:**
- Overhead desnecessário
- Requer conhecimento SQL
- Mais lento para estruturas simples como projeto

#### ❌ Alternativa 4: Binary Serialization
```csharp
var formatter = new BinaryFormatter(); // ⚠️ DEPRECADO!
```
**Problemas:**
- BinaryFormatter foi marcado como obsoleto (.NET 5+)
- Não é portável entre versões .NET
- Não é human-readable

### ✅ Por Que Newtonsoft.Json é Melhor
- Melhor balanço entre **simplicidade** (JSON vs XML) e **robustez** (converters customizados)
- Padrão consolidado em comunidade C#
- Arquivo pode ser inspecionado manualmente
- Performance adequada para escopo do projeto

### 💡 Exemplo: Custom Converters Necessários

```csharp
// 1. GuidConverter: Serializa Guid como string
public class GuidConverter : JsonConverter<Guid>
{
    public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return Guid.Parse((string)reader.Value);
    }
    
    public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}

// 2. IndexColumnsConverter: Serializa apenas IDs de colunas (evita referências circulares)
public class IndexColumnsConverter : JsonConverter<List<Column>>
{
    // Serializa apenas Column.Id
    // Na desserialização, reconstrói referências
}
```

---

## 3. Tradução de Tipos SQL: `DataTypeMapper` (Camada Dedicada)

### ✅ Decisão Escolhida
Criar uma **camada `DataTypeMapper`** que traduz tipos neutros para SQL Server/Oracle.

### 🎯 Justificativa
- **Desacoplamento:** UI e modelos não conhecem dialetos SQL
- **Manutenibilidade:** Mudanças de mapeamento em único lugar
- **Extensibilidade:** Adicionar novo SGBD = estender DataTypeMapper
- **Reutilização:** Mappers pode ser usado por múltiplos geradores

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: Tradução Direta nos Geradores
```csharp
// SqlServerGenerator.cs
public string GenerateScripts(Project project)
{
    foreach (var table in project.Tables)
    {
        foreach (var col in table.Columns)
        {
            if (col.DataType == "STRING")
                sqlScript += $"[{col.Name}] VARCHAR({col.Size ?? 255})";
            else if (col.DataType == "INTEGER")
                sqlScript += $"[{col.Name}] INT";
            // ... muitos if/else duplicados
        }
    }
}
```
**Problemas:**
- Código duplicado entre geradores (SQL Server e Oracle)
- Difícil manter consistência de mapeamento
- Cada gerador tem que "saber" todos os mapeamentos

#### ❌ Alternativa 2: Configuração em Arquivo .json Externo
```json
// mapping-config.json
{
    "STRING": {
        "sqlserver": "VARCHAR",
        "oracle": "VARCHAR2"
    },
    "INTEGER": {
        "sqlserver": "INT",
        "oracle": "NUMBER(10)"
    }
}
```
**Problemas:**
- Overhead desnecessário
- Requer parsing em tempo de execução
- Usuário final não precisa configurar

#### ❌ Alternativa 3: Enums Específicas por SGBD
```csharp
public enum SqlServerDataType { Varchar, Int, Decimal, ... }
public enum OracleDataType { Varchar2, Number, Date, ... }
```
**Problemas:**
- Necessário conversão dupla (Neutral → Specific → SQL)
- Tipo system menos limpo
- Mais acoplamento entre camadas

### ✅ Por Que DataTypeMapper é Melhor
- **Centralização:** Um único lugar para mapeamentos
- **Lógica clara:** Métodos explícitos (`MapToSqlServer`, `MapToOracle`)
- **Testabilidade:** Métodos estáticos podem ser testados isoladamente
- **Performance:** Sem overhead de arquivo externo ou reflexão

### 💡 Exemplo: Implementação

```csharp
public static class DataTypeMapper
{
    // Tipos Neutros → SQL Server
    public static string MapToSqlServer(string neutralType, int? size, int? precision)
    {
        return neutralType.ToUpper() switch
        {
            "STRING" => $"VARCHAR({size ?? 255})",
            "INTEGER" => "INT",
            "DECIMAL" => $"DECIMAL({size ?? 10},{precision ?? 2})",
            "DATE" => "DATETIME",
            "BOOLEAN" => "BIT",
            _ => throw new ArgumentException($"Tipo não suportado: {neutralType}")
        };
    }
    
    // Tipos Neutros → Oracle
    public static string MapToOracle(string neutralType, int? size, int? precision)
    {
        return neutralType.ToUpper() switch
        {
            "STRING" => $"VARCHAR2({size ?? 255})",
            "INTEGER" => "NUMBER(10)",
            "DECIMAL" => $"DECIMAL({size ?? 10},{precision ?? 2})",
            "DATE" => "DATE",
            "BOOLEAN" => "CHAR(1)",
            _ => throw new ArgumentException($"Tipo não suportado: {neutralType}")
        };
    }
}
```

---

## 4. Padrão de Geração SQL: Strategy Pattern

### ✅ Decisão Escolhida
Usar **Strategy Pattern** com interface `IScriptGenerator` e implementações para cada SGBD.

### 🎯 Justificativa
- **Open/Closed Principle:** Aberto para extensão (novo SGBD), fechado para modificação
- **Loose Coupling:** Cada gerador é independente
- **Polimorfismo:** MainForm não precisa conhecer tipo específico de gerador

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: If/Else Gigante
```csharp
public string GenerateScripts(Project project, string sgbd)
{
    if (sgbd == "SqlServer")
        return GenerateSqlServer(project);
    else if (sgbd == "Oracle")
        return GenerateOracle(project);
    else if (sgbd == "PostgreSQL")
        return GeneratePostgreSQL(project);
    // ... crescente com cada novo SGBD
}
```
**Problemas:**
- Violação de Open/Closed Principle
- Difícil manter
- Requer modificação de método existente para cada novo SGBD

#### ❌ Alternativa 2: Reflection
```csharp
var generatorType = Type.GetType($"Scriptor.Generators.{sgbd}Generator");
var generator = (IScriptGenerator)Activator.CreateInstance(generatorType);
```
**Problemas:**
- Overhead de reflexão em tempo de execução
- Menos performático
- Difícil debugar

#### ✅ Strategy Pattern é Melhor
- Explícito e legível
- Nenhum overhead de reflexão
- Fácil adicionar novo gerador: criar nova classe, registrar em MainForm

### 💡 Exemplo: Implementação

```csharp
// Interface
public interface IScriptGenerator
{
    string GenerateScripts(Project project);
}

// Implementações
public class SqlServerGenerator : IScriptGenerator
{
    public string GenerateScripts(Project project) { ... }
}

public class OracleGenerator : IScriptGenerator
{
    public string GenerateScripts(Project project) { ... }
}

// Uso em MainForm
IScriptGenerator generator = new SqlServerGenerator();
string ddl = generator.GenerateScripts(currentProject);
```

---

## 5. Validação de Nomenclatura: Whitelist vs Blacklist

### ✅ Decisão Escolhida
Usar **Whitelist** — nomes só podem conter: letras, números, underscore (`_`).

### 🎯 Justificativa
- **Segurança:** Evita injeção SQL acidental
- **Compatibilidade:** Válido em todos os SGBDs
- **Simplicidade:** Regex simples: `^[a-zA-Z0-9_]+$`

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: Blacklist (Bloquear Caracteres "Ruins")
```csharp
var invalidChars = new[] { '@', '#', '$', '%', '&', '*', '(', ')' };
```
**Problemas:**
- Impossível listar todos caracteres inválidos
- Novo caractere problemático surge, precisa atualizar
- Menos seguro

#### ❌ Alternativa 2: Sem Validação (Deixar SQL Server/Oracle Rejeitar)
```csharp
// Sem validação em aplicação
// Deixar erro ocorrer quando usuário gera script
```
**Problemas:**
- Experiência do usuário ruim (erro só aparece ao gerar)
- Confuso debugar
- Dados inválidos persistem em JSON

### ✅ Por Que Whitelist é Melhor
- **Princípio da Menor Autoridade:** Apenas caracteres conhecidos como seguros
- **Feedback Imediato:** Usuário vê erro ao digitar
- **Segurança:** Previne injeção

---

## 6. Cascata de Exclusão: Automática vs Manual

### ✅ Decisão Escolhida
**Automática com Confirmação:** Quando coluna é removida, indexada em índices/FKs são removidas automaticamente COM alerta.

### 🎯 Justificativa
- **UX:** Usuário não quer excluir dados manualmente
- **Integridade:** Cascata automática evita estados inválidos (coluna em índice que não existe)
- **Alerta:** Mensagem avisa que cascata ocorreu

### 📊 Alternativas Consideradas

#### ❌ Alternativa 1: Impedir Exclusão (Bloqueio)
```csharp
if (column é usada em índice ou FK)
    throw new Exception("Não é possível remover coluna que está em uso");
```
**Problemas:**
- Usuário frustrado (quer remover tabela inteira, não pode remover coluna)
- Requer remover índice/FK ANTES de coluna (overhead)
- Não é user-friendly

#### ❌ Alternativa 2: Cascata Silenciosa (Sem Alerta)
```csharp
RemoveColumnFromAllIndexes();  // Sem log
RemoveColumnFromAllForeignKeys();  // Sem alerta
```
**Problemas:**
- Usuário não sabe que índices/FKs foram removidos
- Dados perdidos sem ciência
- Confusão ao ver script gerado sem índice esperado

### ✅ Cascata com Alerta é Melhor
- **Balanceamento:** Automação (UX) + Transparência (aviso)
- **Segurança:** Usuário sabe que aconteceu
- **Eficiência:** Menos cliques do usuário

### 💡 Exemplo: Implementação

```csharp
public void RemoveColumn(Table table, Column column)
{
    // 1. Verificar uso
    var indexesUsing = ValidationService.CheckColumnUsageInIndexes(column);
    var fksUsing = ValidationService.CheckColumnUsageInForeignKeys(column);
    
    // 2. Alerta
    if (indexesUsing.Count > 0 || fksUsing.Count > 0)
    {
        var msg = $"Coluna está em {indexesUsing.Count} índices e {fksUsing.Count} FKs. Remover?";
        var result = MessageBox.Show(msg, "Confirmar", MessageBoxButtons.YesNo);
        
        if (result != DialogResult.Yes)
            return; // Cancelar
    }
    
    // 3. Cascata
    foreach (var index in indexesUsing)
        index.Columns.Remove(column);
    
    foreach (var fk in fksUsing)
        fk.FieldsForeignKey.RemoveAll(f => f.ColumnId == column.Id);
    
    // 4. Remover coluna
    table.Columns.Remove(column);
}
```

---

## 📊 Resumo de Decisões

| Aspecto | Decisão | Alternativa Rejeitada | Por Que |
|--------|---------|----------------------|--------|
| **Estado Global** | Singleton ProjectService | DI Container | Simplicidade vs complexidade |
| **Persistência** | Newtonsoft.Json + Converters | System.Text.Json | Maduridade + comunidade |
| **Mapeamento SQL** | DataTypeMapper (camada) | Direto em Geradores | Centralização + reutilização |
| **Geração SQL** | Strategy Pattern | If/Else | Open/Closed Principle |
| **Nomenclatura** | Whitelist (letras + números + _) | Blacklist | Segurança |
| **Cascata** | Automática + Alerta | Bloqueio / Silenciosa | UX + Transparência |

---

## 🔄 Revisão de Decisões

Estas decisões são **definitivas** para o escopo atual, mas podem ser revisadas se:
- Novos requisitos emergirem (ex: suporte web → DI Container)
- Performance se tornar problema (ex: JSON muito grande → SQLite)
- Comunidade mudar padrões (ex: System.Text.Json se tornar dominante)

**Última Atualização:** 2026-06-13
