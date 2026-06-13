# Fase 4: Geradores de Scripts SQL (Strategy Pattern)

## 📌 Objetivo

Implementar a interface `IScriptGenerator` e duas implementações concretas (`SqlServerGenerator`, `OracleGenerator`) que geram scripts DDL completos a partir do modelo.

**Dependências:** Fase 2 (Serviços), Fase 3 (Mappers)  
**Status:** ⏳ Não iniciado

---

## 📐 Implementações

### 1. `Generators/IScriptGenerator.cs`

Interface que define o contrato para geradores de scripts.

```csharp
using Scriptor.Models;

namespace Scriptor.Generators
{
    public interface IScriptGenerator
    {
        /// <summary>
        /// Gera scripts DDL (CREATE TABLE, CREATE INDEX, ALTER TABLE FK)
        /// </summary>
        string GenerateScripts(Project project);
    }
}
```

**Método Único:**
- `GenerateScripts(project)` — Retorna string contendo todo DDL

---

### 2. `Generators/SqlServerGenerator.cs`

Gerador para SQL Server.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scriptor.Mappers;
using Scriptor.Models;

namespace Scriptor.Generators
{
    public class SqlServerGenerator : IScriptGenerator
    {
        public string GenerateScripts(Project project)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"-- SQL Server DDL Script");
            sb.AppendLine($"-- Project: {project.NameProject}");
            sb.AppendLine($"-- Client: {project.NameClient}");
            sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (project.Tables == null || project.Tables.Count == 0)
            {
                sb.AppendLine("-- Nenhuma tabela definida");
                return sb.ToString();
            }

            // 1. Gerar CREATE TABLE statements
            foreach (var table in project.Tables)
            {
                sb.Append(GenerateCreateTable(table));
                sb.AppendLine();
            }

            // 2. Gerar CREATE INDEX statements
            foreach (var table in project.Tables)
            {
                foreach (var index in table.Indexes)
                {
                    sb.Append(GenerateCreateIndex(table, index));
                    sb.AppendLine();
                }
            }

            // 3. Gerar ALTER TABLE ADD FOREIGN KEY statements
            foreach (var table in project.Tables)
            {
                foreach (var fk in table.Fks)
                {
                    sb.Append(GenerateAlterTableFK(project, table, fk));
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string GenerateCreateTable(Table table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.Name}] (");

            var columnLines = new List<string>();

            foreach (var column in table.Columns)
            {
                var line = $"    [{column.Name}] {DataTypeMapper.MapToSqlServer(column.DataType, column.Size, column.Precision)}";

                // PK
                if (column.IsPrimaryKey)
                    line += " PRIMARY KEY";

                // NOT NULL
                if (!column.AllowNull)
                    line += " NOT NULL";
                else
                    line += " NULL";

                columnLines.Add(line);
            }

            sb.AppendLine(string.Join(",\n", columnLines));
            sb.AppendLine(");");

            return sb.ToString();
        }

        private string GenerateCreateIndex(Table table, Index index)
        {
            if (index.Columns == null || index.Columns.Count == 0)
                return "";

            var columnNames = string.Join(", ", index.Columns.Select(c => $"[{c.Name}]"));
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE NONCLUSTERED INDEX [{index.Name}] ON [{table.Name}] ({columnNames});");

            return sb.ToString();
        }

        private string GenerateAlterTableFK(Project project, Table table, ForeignKey fk)
        {
            if (fk.FieldsForeignKey == null || fk.FieldsForeignKey.Count == 0)
                return "";

            var referencedTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
            if (referencedTable == null)
                return "";

            var localColumns = string.Join(", ", fk.FieldsForeignKey.Select(f =>
            {
                var col = table.Columns.FirstOrDefault(c => c.Id == f.ColumnId);
                return col != null ? $"[{col.Name}]" : "";
            }));

            var refColumns = string.Join(", ", fk.FieldsForeignKey.Select(f =>
            {
                var col = referencedTable.Columns.FirstOrDefault(c => c.Id == f.ReferencedColumnId);
                return col != null ? $"[{col.Name}]" : "";
            }));

            var fkName = $"FK_{table.Name}_{referencedTable.Name}";

            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE [{table.Name}]");
            sb.AppendLine($"ADD CONSTRAINT [{fkName}] FOREIGN KEY ({localColumns})");
            sb.AppendLine($"REFERENCES [{referencedTable.Name}] ({refColumns});");

            return sb.ToString();
        }
    }
}
```

#### Saída Esperada (SQL Server)
```sql
-- SQL Server DDL Script
-- Project: CRM System
-- Client: Acme Corp
-- Generated: 2026-06-13 10:30:45

CREATE TABLE [Users] (
    [Id] INT PRIMARY KEY NOT NULL,
    [Email] VARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME NULL
);

CREATE NONCLUSTERED INDEX [IDX_Users_Email] ON [Users] ([Email]);

ALTER TABLE [Orders]
ADD CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId])
REFERENCES [Users] ([Id]);
```

---

### 3. `Generators/OracleGenerator.cs`

Gerador para Oracle.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scriptor.Mappers;
using Scriptor.Models;

namespace Scriptor.Generators
{
    public class OracleGenerator : IScriptGenerator
    {
        public string GenerateScripts(Project project)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"-- Oracle DDL Script");
            sb.AppendLine($"-- Project: {project.NameProject}");
            sb.AppendLine($"-- Client: {project.NameClient}");
            sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (project.Tables == null || project.Tables.Count == 0)
            {
                sb.AppendLine("-- Nenhuma tabela definida");
                return sb.ToString();
            }

            // 1. Gerar CREATE TABLE statements
            foreach (var table in project.Tables)
            {
                sb.Append(GenerateCreateTable(table));
                sb.AppendLine();
            }

            // 2. Gerar CREATE INDEX statements
            foreach (var table in project.Tables)
            {
                foreach (var index in table.Indexes)
                {
                    sb.Append(GenerateCreateIndex(table, index));
                    sb.AppendLine();
                }
            }

            // 3. Gerar ALTER TABLE ADD CONSTRAINT FOREIGN KEY
            foreach (var table in project.Tables)
            {
                foreach (var fk in table.Fks)
                {
                    sb.Append(GenerateAlterTableFK(project, table, fk));
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string GenerateCreateTable(Table table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {table.Name} (");

            var columnLines = new List<string>();

            foreach (var column in table.Columns)
            {
                var line = $"    {column.Name} {DataTypeMapper.MapToOracle(column.DataType, column.Size, column.Precision)}";

                // PK
                if (column.IsPrimaryKey)
                    line += " PRIMARY KEY";

                // NOT NULL
                if (!column.AllowNull)
                    line += " NOT NULL";

                columnLines.Add(line);
            }

            sb.AppendLine(string.Join(",\n", columnLines));
            sb.AppendLine(");");

            return sb.ToString();
        }

        private string GenerateCreateIndex(Table table, Index index)
        {
            if (index.Columns == null || index.Columns.Count == 0)
                return "";

            var columnNames = string.Join(", ", index.Columns.Select(c => c.Name));
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE INDEX {index.Name} ON {table.Name} ({columnNames});");

            return sb.ToString();
        }

        private string GenerateAlterTableFK(Project project, Table table, ForeignKey fk)
        {
            if (fk.FieldsForeignKey == null || fk.FieldsForeignKey.Count == 0)
                return "";

            var referencedTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
            if (referencedTable == null)
                return "";

            var localColumns = string.Join(", ", fk.FieldsForeignKey.Select(f =>
            {
                var col = table.Columns.FirstOrDefault(c => c.Id == f.ColumnId);
                return col != null ? col.Name : "";
            }));

            var refColumns = string.Join(", ", fk.FieldsForeignKey.Select(f =>
            {
                var col = referencedTable.Columns.FirstOrDefault(c => c.Id == f.ReferencedColumnId);
                return col != null ? col.Name : "";
            }));

            var fkName = $"FK_{table.Name}_{referencedTable.Name}";

            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {table.Name}");
            sb.AppendLine($"ADD CONSTRAINT {fkName} FOREIGN KEY ({localColumns})");
            sb.AppendLine($"REFERENCES {referencedTable.Name} ({refColumns});");

            return sb.ToString();
        }
    }
}
```

#### Saída Esperada (Oracle)
```sql
-- Oracle DDL Script
-- Project: CRM System
-- Client: Acme Corp
-- Generated: 2026-06-13 10:30:45

CREATE TABLE Users (
    Id NUMBER(10) PRIMARY KEY NOT NULL,
    Email VARCHAR2(255) NOT NULL,
    CreatedAt DATE
);

CREATE INDEX IDX_Users_Email ON Users (Email);

ALTER TABLE Orders
ADD CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId)
REFERENCES Users (Id);
```

---

## 📁 Estrutura de Arquivos a Criar

```
Scriptor/Generators/
├── IScriptGenerator.cs
├── SqlServerGenerator.cs
└── OracleGenerator.cs
```

---

## 🔧 Checklist de Implementação

### IScriptGenerator.cs
- [ ] Interface `IScriptGenerator`
- [ ] Método `GenerateScripts(project) → string`

### SqlServerGenerator.cs
- [ ] Implements `IScriptGenerator`
- [ ] Método `GenerateScripts()` — Orquestra geração
- [ ] Método privado `GenerateCreateTable(table)` — CREATE TABLE
- [ ] Método privado `GenerateCreateIndex(table, index)` — CREATE INDEX
- [ ] Método privado `GenerateAlterTableFK()` — ALTER TABLE FK
- [ ] Usa `DataTypeMapper.MapToSqlServer()`
- [ ] Formata com comentários e espaçamento

### OracleGenerator.cs
- [ ] Implements `IScriptGenerator`
- [ ] Método `GenerateScripts()` — Orquestra geração
- [ ] Método privado `GenerateCreateTable(table)` — CREATE TABLE
- [ ] Método privado `GenerateCreateIndex(table, index)` — CREATE INDEX
- [ ] Método privado `GenerateAlterTableFK()` — ALTER TABLE FK
- [ ] Usa `DataTypeMapper.MapToOracle()`
- [ ] Formata com comentários e espaçamento

---

## ✅ Verificação e Testes

### Teste 1: Gerar DDL SQL Server
```csharp
var generator = new SqlServerGenerator();
var ddl = generator.GenerateScripts(project);

Assert.IsTrue(ddl.Contains("CREATE TABLE"));
Assert.IsTrue(ddl.Contains("SQL Server"));
```

### Teste 2: Gerar DDL Oracle
```csharp
var generator = new OracleGenerator();
var ddl = generator.GenerateScripts(project);

Assert.IsTrue(ddl.Contains("CREATE TABLE"));
Assert.IsTrue(ddl.Contains("Oracle"));
```

### Teste 3: Formação de CREATE TABLE
```csharp
var table = new Table { Name = "Users" };
table.Columns.Add(new Column { Name = "Id", DataType = "INTEGER", IsPrimaryKey = true });

var generator = new SqlServerGenerator();
var ddl = generator.GenerateScripts(new Project { Tables = new List<Table> { table } });

Assert.IsTrue(ddl.Contains("[Users]"));
Assert.IsTrue(ddl.Contains("[Id] INT PRIMARY KEY"));
```

### Teste 4: Índices
```csharp
var index = new Index { Name = "IDX_Email" };
var column = new Column { Name = "Email", Id = Guid.NewGuid() };
index.Columns.Add(column);

table.Indexes.Add(index);

var ddl = generator.GenerateScripts(new Project { Tables = new List<Table> { table } });
Assert.IsTrue(ddl.Contains("CREATE NONCLUSTERED INDEX [IDX_Email]"));
```

---

## 📝 Notas Importantes

### Formatação
- SQL Server: Coluna entre colchetes `[NomeDaColuna]`
- Oracle: Sem colchetes `NomeDaColuna`

### Índices
- SQL Server: `CREATE NONCLUSTERED INDEX` (indicar tipo)
- Oracle: `CREATE INDEX` (simples)

### Foreign Keys
- Ambos: Usar `ALTER TABLE ... ADD CONSTRAINT ... FOREIGN KEY`
- Nomes de constraint: `FK_{TableLocal}_{TableReferenciada}`

### Tratamento de Dados Nulos
- Colunas com `AllowNull = true` → sem `NOT NULL`
- Colunas com `AllowNull = false` → com `NOT NULL`
- Oracle não coloca `NULL` explicitamente, SQL Server sim

---

## 🔗 Próximos Passos

Após concluir Fase 4:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 5: UI Principal** (`05_phase_mainform.md`)

**Tempo Estimado:** 1.5-2 horas

**Verificação Final:**
- [ ] Compilar sem erros
- [ ] 4 testes acima executados com sucesso
- [ ] DDL gerado é sintaticamente válido (pode testar em SSMS ou SQL*Plus)
