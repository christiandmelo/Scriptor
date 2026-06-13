# Arquitetura: Gerador de Scripts SQL WinForms

## 🏗️ Visão Geral da Arquitetura

O projeto segue uma arquitetura em **camadas** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────┐
│                     UI Layer (WinForms)                     │
│  ┌─────────────┬──────────────────┬────────────────────────┐
│  │ MainForm    │ TableEditorForm   │ ForeignKeyModalForm    │
│  └─────────────┴──────────────────┴────────────────────────┘
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   Business Logic Layer                      │
│  ┌──────────────┬────────────────┬──────────────────────────┐
│  │ ProjectSvc   │ ValidationSvc   │ JsonSerializationSvc     │
│  └──────────────┴────────────────┴──────────────────────────┘
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              Domain Models & Utilities Layer                │
│  ┌──────────┬────────┬─────────┬──────────┬─────────────────┐
│  │ Project  │ Table  │ Column  │ Index   │ ForeignKey       │
│  └──────────┴────────┴─────────┴──────────┴─────────────────┘
│  ┌──────────────────┬─────────────────────┐                 │
│  │ DataTypeMapper   │ Constants/Enums     │                 │
│  └──────────────────┴─────────────────────┘                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│           Generator Layer (Strategy Pattern)                │
│  ┌─────────────────────┬────────────────────────────────────┐
│  │ IScriptGenerator    │ Implementações:                    │
│  │ (Interface)         │ • SqlServerGenerator               │
│  │                     │ • OracleGenerator                  │
│  └─────────────────────┴────────────────────────────────────┘
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              Data Persistence Layer                         │
│  ┌───────────────────────────────────────────────────────────┐
│  │ JSON File (Local Disk)                                    │
│  │ Serialized via Newtonsoft.Json                            │
│  └───────────────────────────────────────────────────────────┘
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Componentes por Camada

### 1. **UI Layer (WinForms)**
Responsável pela interação com o usuário. Cada formulário é responsável por exibir dados e capturar ações do usuário, delegando lógica para a camada de serviços.

**Componentes:**
- `MainForm.cs` — Tela principal com menu, contexto e DataGridView de tabelas
- `TableEditorForm.cs` — Editor de tabelas com 3 abas (Colunas, Índices, FKs)
- `ForeignKeyModalForm.cs` — Modal para configurar chaves estrangeiras

**Responsabilidades:**
- Renderizar dados na UI
- Capturar eventos de usuário (clique, duplo clique, entrada de texto)
- Chamar serviços para salvar/carregar dados
- Exibir mensagens (confirmação, erro, sucesso)

### 2. **Business Logic Layer (Services)**
Camada de regras de negócio e orquestração de dados.

**Componentes:**

#### `ProjectService` (Singleton)
- **Responsabilidade:** Gerenciar estado global do projeto em memória
- **Métodos principais:**
  - `GetInstance()` — Retorna instância Singleton
  - `GetCurrentProject()` — Retorna `Project` em memória
  - `LoadProject(filePath)` — Carrega do JSON
  - `SaveProject(filePath)` — Salva para JSON
  - `CreateNewProject(clientName, projectName)` — Cria novo projeto
  - `AddTable(table)` — Adiciona tabela
  - `RemoveTable(tableId)` — Remove tabela
  - `UpdateTable(table)` — Atualiza tabela existente

#### `ValidationService`
- **Responsabilidade:** Validar dados conforme regras de negócio
- **Métodos principais:**
  - `ValidateTableName(name)` — Valida nomenclatura de tabela
  - `ValidateColumnName(name)` — Valida nomenclatura de coluna
  - `IsNameReservedWord(name)` — Verifica palavras reservadas SQL
  - `ValidateForeignKeyMapping(column1, column2)` — Valida compatibilidade de tipos
  - `CheckColumnUsageInIndexes(column)` — Retorna índices que usam coluna
  - `CheckColumnUsageInForeignKeys(column)` — Retorna FKs que usam coluna

#### `JsonSerializationService`
- **Responsabilidade:** Serializar/desserializar `Project` para JSON
- **Métodos principais:**
  - `SerializeToJson(project)` — Converte para string JSON
  - `SerializeToFile(project, filePath)` — Salva em arquivo
  - `DeserializeFromJson(jsonString)` — Converte de string JSON
  - `DeserializeFromFile(filePath)` — Carrega de arquivo
- **Custom Converters:**
  - `GuidConverter` — Serializa `Guid` como string
  - `IndexColumnsConverter` — Serializa apenas IDs de colunas em índices

### 3. **Domain Models Layer**
Classes que representam o domínio de negócio.

**Componentes:**

```csharp
// Estrutura hierárquica
Project
  └── Tables (List<Table>)
       ├── Columns (List<Column>)
       ├── Indexes (List<Index>)
       │    └── Columns (List<Column>) — Referências
       └── ForeignKeys (List<ForeignKey>)
            └── FieldsForeignKey (List<FieldsForeignKey>)
                 ├── ColumnId (guid)
                 └── ReferencedColumnId (guid)
```

### 4. **Mappers Layer**
Camada de transformação de tipos de dados.

**Componentes:**

#### `DataTypeMapper`
- **Responsabilidade:** Traduzir tipos neutros (STRING, INTEGER, DATE) para SQL Server/Oracle
- **Métodos principais:**
  - `MapToSqlServer(neutralType, size, precision)` → "VARCHAR(100)"
  - `MapToOracle(neutralType, size, precision)` → "VARCHAR2(100)"
  - `GetDefaultSize(neutralType)` → Retorna tamanho padrão
  - `IsValidTypeForSqlServer(sqlType)` → bool
  - `IsValidTypeForOracle(oracleType)` → bool

**Tipos Neutros Suportados:**
- `STRING` / `VARCHAR`
- `INTEGER` / `INT`
- `DECIMAL` / `NUMERIC`
- `DATE`
- `BOOLEAN`
- `TEXT`
- `BLOB`

### 5. **Generator Layer (Strategy Pattern)**
Responsável por gerar DDL conforme SGBD específico.

**Padrão:** Strategy Pattern
```csharp
interface IScriptGenerator
{
    string GenerateScripts(Project project);
}

// Implementações
class SqlServerGenerator : IScriptGenerator { ... }
class OracleGenerator : IScriptGenerator { ... }
```

**Responsabilidades:**
- Gerar `CREATE TABLE` com tipos específicos do SGBD
- Gerar `CREATE INDEX` (CLUSTERED/NONCLUSTERED para SQL Server, simples para Oracle)
- Gerar `ALTER TABLE ADD FOREIGN KEY` com constraints apropriadas
- Formatar SQL legível (indentação, quebras de linha)

### 6. **Utilities Layer**
Constantes, enums e helpers.

**Componentes:**

#### `Constants.cs`
- Lista de palavras reservadas SQL (SELECT, FROM, WHERE, TABLE, INDEX, etc.)
- Caracteres não permitidos em nomes (espaço, @, #, $, %, &, *, etc.)
- Tamanhos padrão para tipos (VARCHAR = 255, DECIMAL = 10,2, etc.)

#### `NeutralDataType.cs`
- Enum com tipos de dados suportados (STRING, INTEGER, DECIMAL, DATE, BOOLEAN, TEXT, BLOB)

### 7. **Data Persistence Layer**
Persistência em arquivo JSON local.

**Componentes:**
- Arquivo `.json` no disco local (usuário especifica localização)
- Estrutura de dados serializada com Newtonstein.Json
- Custom converters para tipos complexos (Guid, referências circulares)

---

## 🔄 Fluxos de Dados (Use Cases)

### Fluxo 1: Criar Novo Projeto
```
1. Usuário clica "Arquivo → Novo Projeto"
2. MainForm solicita nome do cliente e projeto
3. ProjectService.CreateNewProject(clientName, projectName)
4. Novo Project é instanciado em memória
5. MainForm atualiza DataGridView (vazio, sem tabelas)
6. Estado pronto para adicionar tabelas
```

### Fluxo 2: Adicionar Tabela e Colunas
```
1. Usuário clica "Nova Tabela" em MainForm
2. TableEditorForm abre em modo "novo"
3. Usuário insere nome da tabela
4. Usuário clica "Adicionar Coluna" na Aba 1
5. TableEditorForm.AddColumn() insere coluna vazia
6. Usuário preenche Nome, Tipo, Tamanho, etc.
7. Usuário clica "Salvar Tabela"
8. Validação ocorre em ValidationService
9. ProjectService.AddTable(table)
10. MainForm atualiza DataGridView com nova tabela
```

### Fluxo 3: Gerar Scripts SQL
```
1. Usuário clica "Ações → Gerar Scripts SQL Server"
2. MainForm obtém Project via ProjectService.GetCurrentProject()
3. SqlServerGenerator.GenerateScripts(project) é chamado
4. Iteração sobre tabelas e geração de DDL
5. DDL formatado é exibido em um diálogo ou TextBox
6. Usuário pode copiar ou salvar em arquivo .sql
```

### Fluxo 4: Salvar Projeto
```
1. Usuário clica "Arquivo → Salvar Projeto"
2. SaveFileDialog permite escolher localização
3. ProjectService.SaveProject(filePath) é chamado
4. JsonSerializationService serializa Project para JSON
5. Arquivo salvo em disco
6. Mensagem de sucesso exibida
```

### Fluxo 5: Carregar Projeto
```
1. Usuário clica "Arquivo → Carregar Projeto"
2. OpenFileDialog permite escolher arquivo .json
3. JsonSerializationService.DeserializeFromFile(filePath) carrega
4. Custom converters restauram Guids e referências
5. ProjectService.LoadProject(project) atualiza estado em memória
6. MainForm atualiza UI com dados carregados
```

---

## 🔐 Integridade de Dados

### Validações em Múltiplas Camadas

**Camada 1: UI (WinForms)**
- Validação em tempo real (ex: TextBox só aceita caracteres válidos)
- Desabilitar botões quando dados inválidos

**Camada 2: Services**
- Validação completa antes de persistir (ValidationService)
- Verificação de integridade referencial (FKs órfãs, colunas não existentes)

**Camada 3: Persistência**
- Custom converters detectam dados corrompidos
- Erros de desserialização resultam em exceção e log

### Cascata de Exclusão

Quando uma coluna é removida:
1. `ValidationService.CheckColumnUsageInIndexes()` retorna índices afetados
2. Coluna é removida do `Index.Columns` de cada índice
3. `ValidationService.CheckColumnUsageInForeignKeys()` retorna FKs afetadas
4. Usuário recebe alerta: "Coluna está em X FKs. Remover?"
5. Se sim, `FieldsForeignKey` correspondentes são removidas
6. Se FK fica vazia, a FK inteira é removida

---

## 🎯 Padrões de Design

### 1. Singleton (ProjectService)
- Uma única instância gerencia estado global
- Acessível de qualquer Form via `ProjectService.GetInstance()`
- Garante Single Source of Truth

### 2. Strategy Pattern (IScriptGenerator)
- Interface define contrato para geradores
- Implementações concretas para cada SGBD
- Fácil adicionar novos geradores no futuro

### 3. Repository Pattern (Implícito)
- `ProjectService` atua como "repositório" em memória
- `JsonSerializationService` fornece persistência

### 4. Facade Pattern (ProjectService)
- Simplifica acesso a múltiplas operações
- Orquestra chamadas a ValidationService, JsonSerializationService, etc.

---

## 📊 Diagrama de Dependências

```
UI Layer
  ↓ (usa)
Services Layer (ProjectService, ValidationService, JsonSerializationService)
  ↓ (usa)
Domain Models (Project, Table, Column, Index, FK, FieldsForeignKey)
  ↓ (usa)
Mappers Layer (DataTypeMapper)
  ↓ (usa)
Generators Layer (IScriptGenerator, SqlServerGenerator, OracleGenerator)
  ↓ (produz)
DDL SQL String
  ↓ (salva)
JSON File (Persistência)
```

---

## ✅ Benefícios da Arquitetura

- **Separação de Responsabilidades:** Cada camada tem responsabilidade clara
- **Testabilidade:** Serviços podem ser testados independentemente da UI
- **Manutenibilidade:** Fácil localizar e corrigir bugs
- **Extensibilidade:** Adicionar novo SGBD = criar novo gerador
- **Reutilização:** Serviços podem ser reutilizados em diferentes contextos (UI, API, CLI)
- **Escalabilidade:** Arquitetura suporta crescimento sem refatoração major

---

## 🔗 Próximos Passos

- Revisar decisões arquiteturais em `decisions.md`
- Iniciar Fase 1 (Modelos Base) em `01_phase_models.md`
