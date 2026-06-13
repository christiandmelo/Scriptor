# Fase 1: Modelos Base (Domain Models)

## 📌 Objetivo

Definir todas as classes que representam o domínio de negócio (Project, Table, Column, Index, ForeignKey, FieldsForeignKey). Essas classes formam a **estrutura de dados central** que será manipulada pela aplicação.

**Dependências:** Nenhuma (fase inicial)  
**Status:** ⏳ Não iniciado

---

## 📐 Classes a Implementar

### 1. `Project.cs`
Representa um projeto completo com informações do cliente.

```csharp
public class Project
{
    public string NameClient { get; set; }
    public string NameProject { get; set; }
    public List<Table> Tables { get; set; } = new List<Table>();
}
```

**Propriedades:**
- `NameClient` (string) — Nome do cliente/empresa
- `NameProject` (string) — Nome do projeto de banco de dados
- `Tables` (List<Table>) — Lista de tabelas (inicializada para evitar null reference)

**Notas:**
- Usar inicialização de propriedade (`= new List<>()`) para evitar NullReferenceException

---

### 2. `Table.cs`
Representa uma tabela no banco de dados.

```csharp
public class Table
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }  // Opcional
    public List<Column> Columns { get; set; } = new List<Column>();
    public List<Index> Indexes { get; set; } = new List<Index>();
    public List<ForeignKey> Fks { get; set; } = new List<ForeignKey>();
}
```

**Propriedades:**
- `Id` (Guid) — Identificador único da tabela (gerado automaticamente)
- `Name` (string) — Nome da tabela (validado, sem espaços/caracteres especiais)
- `Description` (string, opcional) — Descrição documentação da tabela
- `Columns` (List<Column>) — Colunas da tabela
- `Indexes` (List<Index>) — Índices da tabela
- `Fks` (List<ForeignKey>) — Chaves estrangeiras da tabela

**Notas:**
- `Id` deve ser único (usado para referências)
- Listas inicializadas para evitar null reference

---

### 3. `Column.cs`
Representa uma coluna de uma tabela.

```csharp
public class Column
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string DataType { get; set; }  // Ex: STRING, INTEGER, DECIMAL, DATE, BOOLEAN
    public int? Size { get; set; }        // Ex: VARCHAR(100) -> Size=100
    public int? Precision { get; set; }   // Ex: DECIMAL(10,2) -> Size=10, Precision=2
    public bool IsPrimaryKey { get; set; }
    public bool AllowNull { get; set; }
}
```

**Propriedades:**
- `Id` (Guid) — Identificador único da coluna
- `Name` (string) — Nome da coluna
- `DataType` (string) — Tipo de dado neutro (STRING, INTEGER, DECIMAL, DATE, BOOLEAN, TEXT, BLOB)
- `Size` (int?, opcional) — Tamanho (ex: VARCHAR(100), DECIMAL(10,2))
- `Precision` (int?, opcional) — Precisão decimal (ex: casas decimais em DECIMAL)
- `IsPrimaryKey` (bool) — Se é parte da chave primária
- `AllowNull` (bool) — Se permite NULL

**Notas:**
- `DataType` não é enum (é string) para flexibilidade
- `Size` e `Precision` são nullable porque nem todos os tipos precisam

---

### 4. `Index.cs`
Representa um índice na tabela.

```csharp
public class Index
{
    public string Name { get; set; }
    public List<Column> Columns { get; set; } = new List<Column>();
}
```

**Propriedades:**
- `Name` (string) — Nome do índice (ex: "IDX_User_Email")
- `Columns` (List<Column>) — Colunas que compõem o índice

**Notas:**
- **IMPORTANTE:** `Columns` armazena **referências** aos objetos Column, não IDs
- Na serialização JSON, apenas IDs são salvos (custom converter)
- Na desserialização JSON, referências são reconstituídas

---

### 5. `ForeignKey.cs`
Representa uma chave estrangeira (relacionamento).

```csharp
public class ForeignKey
{
    public Guid ReferencedTableId { get; set; }
    public List<FieldsForeignKey> FieldsForeignKey { get; set; } = new List<FieldsForeignKey>();
}
```

**Propriedades:**
- `ReferencedTableId` (Guid) — ID da tabela referenciada
- `FieldsForeignKey` (List<FieldsForeignKey>) — Lista de pares campo-local → campo-remoto

**Notas:**
- Suporta FK composta (múltiplas colunas)
- `ReferencedTableId` permite auto-relacionamento (tabela referencia a si mesma)

---

### 6. `FieldsForeignKey.cs`
Representa um par de colunas em uma FK composta.

```csharp
public class FieldsForeignKey
{
    public Guid ColumnId { get; set; }              // ID da coluna nesta tabela
    public Guid ReferencedColumnId { get; set; }    // ID da coluna na tabela referenciada
}
```

**Propriedades:**
- `ColumnId` (Guid) — ID da coluna local
- `ReferencedColumnId` (Guid) — ID da coluna referenciada

**Notas:**
- Apenas IDs são armazenados (sem referências de objeto)
- Simplifica serialização/desserialização
- ValidationService verificará se IDs existem

---

## 📁 Estrutura de Arquivos a Criar

```
Scriptor/Models/
├── Project.cs
├── Table.cs
├── Column.cs
├── Index.cs
├── ForeignKey.cs
└── FieldsForeignKey.cs
```

---

## 🔧 Checklist de Implementação

### Pré-Requisitos
- [ ] Pasta `Scriptor/Models/` criada
- [ ] Projeto compilável (sem erros de build)

### Por Classe

#### Project.cs
- [ ] Propriedade `NameClient` (string)
- [ ] Propriedade `NameProject` (string)
- [ ] Propriedade `Tables` inicializada como `new List<Table>()`
- [ ] Sem lógica (apenas POCO — Plain Old CLR Object)

#### Table.cs
- [ ] Propriedade `Id` com `Guid.NewGuid()` como padrão
- [ ] Propriedade `Name` (string)
- [ ] Propriedade `Description` (string, opcional)
- [ ] Propriedade `Columns` inicializada
- [ ] Propriedade `Indexes` inicializada
- [ ] Propriedade `Fks` inicializada

#### Column.cs
- [ ] Propriedade `Id` com `Guid.NewGuid()`
- [ ] Propriedade `Name` (string)
- [ ] Propriedade `DataType` (string)
- [ ] Propriedade `Size` (int?, nullable)
- [ ] Propriedade `Precision` (int?, nullable)
- [ ] Propriedade `IsPrimaryKey` (bool)
- [ ] Propriedade `AllowNull` (bool)

#### Index.cs
- [ ] Propriedade `Name` (string)
- [ ] Propriedade `Columns` inicializada como `new List<Column>()`

#### ForeignKey.cs
- [ ] Propriedade `ReferencedTableId` (Guid)
- [ ] Propriedade `FieldsForeignKey` inicializada

#### FieldsForeignKey.cs
- [ ] Propriedade `ColumnId` (Guid)
- [ ] Propriedade `ReferencedColumnId` (Guid)

---

## ✅ Verificação e Testes

### Compilação
```bash
# No VS ou terminal, compilar solução
dotnet build
```

**Esperado:** Sem erros de compilação

### Teste Manual: Instanciar Objetos
```csharp
// Program.cs ou teste
var project = new Project
{
    NameClient = "Acme Corp",
    NameProject = "CRM System"
};

var table = new Table
{
    Name = "Users"
};

var column = new Column
{
    Name = "Id",
    DataType = "INTEGER",
    IsPrimaryKey = true,
    AllowNull = false
};

table.Columns.Add(column);
project.Tables.Add(table);

// Verificar que não há null reference
Console.WriteLine($"Projeto: {project.NameProject}");
Console.WriteLine($"Tabelas: {project.Tables.Count}");  // Deve ser 1
Console.WriteLine($"Colunas: {table.Columns.Count}");   // Deve ser 1
```

**Esperado:**
- Projeto: CRM System
- Tabelas: 1
- Colunas: 1

### Checklist de Validação
- [ ] Nenhum erro ao compilar
- [ ] Instâncias podem ser criadas sem NullReferenceException
- [ ] Listas podem receber elementos (.Add())
- [ ] GUIDs são gerados automaticamente
- [ ] Propriedades nullable funcionam (int? Size)

---

## 🔍 Estrutura de Dados Exemplo

```
Project {
  NameClient: "Acme Corp"
  NameProject: "CRM System"
  Tables: [
    Table {
      Id: 550e8400-e29b-41d4-a716-446655440000
      Name: "Users"
      Description: "Usuários do sistema"
      Columns: [
        Column {
          Id: 550e8400-e29b-41d4-a716-446655440001
          Name: "Id"
          DataType: "INTEGER"
          Size: null
          Precision: null
          IsPrimaryKey: true
          AllowNull: false
        },
        Column {
          Id: 550e8400-e29b-41d4-a716-446655440002
          Name: "Email"
          DataType: "STRING"
          Size: 255
          Precision: null
          IsPrimaryKey: false
          AllowNull: false
        }
      ]
      Indexes: [
        Index {
          Name: "IDX_Users_Email"
          Columns: [Column.Id: 550e8400-e29b-41d4-a716-446655440002]
        }
      ]
      Fks: []
    }
  ]
}
```

---

## 📝 Notas Importantes

### Inicialização de Listas
**IMPORTANTE:** Use inicialização em linha para evitar NullReferenceException:
```csharp
// ✅ Correto
public List<Table> Tables { get; set; } = new List<Table>();

// ❌ Errado (pode causar null reference)
public List<Table> Tables { get; set; }
```

### GUIDs como Identificadores
- Cada `Table`, `Column`, e `Index` recebe Guid único
- GUIDs garantem unicidade mesmo após serialização/desserialização
- Evita problemas de IDs incrementais em JSON

### Tipos de Dados Neutros
DataType é string (não enum) porque:
- Flexibilidade (usuário pode adicionar tipos customizados)
- Facilita serialização
- Mappers convertem para tipos específicos após

---

## 🔗 Próximos Passos

Após concluir Fase 1:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 2: Serviços & Persistência** (`02_phase_services.md`)

**Tempo Estimado:** 30-45 minutos
