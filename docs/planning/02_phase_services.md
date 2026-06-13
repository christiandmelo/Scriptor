# Fase 2: Serviços & Persistência

## 📌 Objetivo

Implementar a camada de serviços que gerencia estado (ProjectService), valida dados (ValidationService) e persiste em JSON (JsonSerializationService).

**Dependências:** Fase 1 (Modelos Base)  
**Status:** ⏳ Não iniciado

---

## 🔧 Pré-Requisitos

### NuGet: Adicionar Newtonsoft.Json
```bash
# PowerShell/Terminal na pasta do projeto
dotnet add package Newtonsoft.Json --version 13.0.3
```

Ou via Package Manager Console (Visual Studio):
```
Install-Package Newtonsoft.Json -Version 13.0.3
```

---

## 📦 Serviços a Implementar

### 1. `ProjectService.cs` (Singleton)

**Responsabilidade:** Gerenciar o estado global do projeto em memória. Único ponto de acesso para obter/modificar dados.

#### Implementação Básica

```csharp
using System;
using System.Collections.Generic;
using Scriptor.Models;

namespace Scriptor.Services
{
    public class ProjectService
    {
        private static ProjectService _instance;
        private Project _currentProject;

        // Singleton
        private ProjectService() { }

        public static ProjectService GetInstance()
        {
            if (_instance == null)
                _instance = new ProjectService();
            return _instance;
        }

        // Métodos
        public Project GetCurrentProject()
        {
            if (_currentProject == null)
                _currentProject = new Project();
            return _currentProject;
        }

        public void SetCurrentProject(Project project)
        {
            _currentProject = project ?? throw new ArgumentNullException(nameof(project));
        }

        public void CreateNewProject(string clientName, string projectName)
        {
            _currentProject = new Project
            {
                NameClient = clientName,
                NameProject = projectName
            };
        }

        // Operações em Tabelas
        public void AddTable(Table table)
        {
            GetCurrentProject().Tables.Add(table);
        }

        public void RemoveTable(Guid tableId)
        {
            var table = GetCurrentProject().Tables.Find(t => t.Id == tableId);
            if (table != null)
                GetCurrentProject().Tables.Remove(table);
        }

        public Table GetTableById(Guid tableId)
        {
            return GetCurrentProject().Tables.Find(t => t.Id == tableId);
        }

        public void UpdateTable(Table table)
        {
            var existing = GetTableById(table.Id);
            if (existing != null)
            {
                existing.Name = table.Name;
                existing.Description = table.Description;
                existing.Columns = table.Columns;
                existing.Indexes = table.Indexes;
                existing.Fks = table.Fks;
            }
        }

        // Serialização (delegada a JsonSerializationService)
        public void SaveProject(string filePath)
        {
            JsonSerializationService.SerializeToFile(_currentProject, filePath);
        }

        public void LoadProject(string filePath)
        {
            _currentProject = JsonSerializationService.DeserializeFromFile(filePath);
        }

        public void Clear()
        {
            _currentProject = null;
        }
    }
}
```

#### Métodos Esperados

| Método | Parâmetros | Retorno | Descrição |
|--------|-----------|---------|-----------|
| `GetInstance()` | — | `ProjectService` | Retorna instância Singleton |
| `GetCurrentProject()` | — | `Project` | Retorna projeto em memória (cria novo se null) |
| `SetCurrentProject(project)` | `Project` | void | Define novo projeto em memória |
| `CreateNewProject(clientName, projectName)` | string, string | void | Cria novo projeto vazio |
| `AddTable(table)` | `Table` | void | Adiciona tabela ao projeto |
| `RemoveTable(tableId)` | `Guid` | void | Remove tabela pelo ID |
| `GetTableById(tableId)` | `Guid` | `Table` | Retorna tabela ou null |
| `UpdateTable(table)` | `Table` | void | Atualiza tabela existente |
| `SaveProject(filePath)` | string | void | Salva em arquivo JSON |
| `LoadProject(filePath)` | string | void | Carrega de arquivo JSON |
| `Clear()` | — | void | Limpa estado (útil para testes) |

---

### 2. `ValidationService.cs`

**Responsabilidade:** Validar dados conforme regras de negócio.

#### Implementação Básica

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Scriptor.Models;
using Scriptor.Utilities;

namespace Scriptor.Services
{
    public class ValidationService
    {
        /// <summary>
        /// Valida nome de tabela/coluna/índice
        /// Regras: sem espaços, sem caracteres especiais (exceto _), não é palavra reservada
        /// </summary>
        public static bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Padrão: apenas letras, números, underscore
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$"))
                return false;

            // Não pode ser palavra reservada
            if (IsNameReservedWord(name))
                return false;

            return true;
        }

        public static bool IsNameReservedWord(string name)
        {
            var reservedWords = Constants.RESERVED_WORDS;
            return reservedWords.Contains(name.ToUpper());
        }

        /// <summary>
        /// Verifica em quais índices a coluna está sendo usada
        /// </summary>
        public static List<Index> CheckColumnUsageInIndexes(Table table, Column column)
        {
            return table.Indexes
                .Where(idx => idx.Columns.Any(col => col.Id == column.Id))
                .ToList();
        }

        /// <summary>
        /// Verifica em quais FKs a coluna está sendo usada
        /// </summary>
        public static List<ForeignKey> CheckColumnUsageInForeignKeys(Table table, Column column)
        {
            return table.Fks
                .Where(fk => fk.FieldsForeignKey.Any(f => f.ColumnId == column.Id))
                .ToList();
        }

        /// <summary>
        /// Valida que duas colunas têm o mesmo tipo de dado (para FK)
        /// </summary>
        public static bool ValidateForeignKeyMapping(Column localColumn, Column referencedColumn)
        {
            // Tipos devem ser iguais
            if (localColumn.DataType != referencedColumn.DataType)
                return false;

            // Tamanho deve ser compatível
            if (localColumn.DataType == "STRING" || localColumn.DataType == "DECIMAL")
            {
                if (localColumn.Size != referencedColumn.Size)
                    return false;

                if (localColumn.DataType == "DECIMAL" && localColumn.Precision != referencedColumn.Precision)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica se todas as referências de FK são válidas
        /// </summary>
        public static void ValidateForeignKeyConsistency(Project project)
        {
            foreach (var table in project.Tables)
            {
                var invalidFks = new List<ForeignKey>();

                foreach (var fk in table.Fks)
                {
                    // Verificar se tabela referenciada existe
                    var referencedTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
                    if (referencedTable == null)
                    {
                        invalidFks.Add(fk);
                        continue;
                    }

                    // Verificar se campos existem
                    var invalidFields = new List<FieldsForeignKey>();
                    foreach (var field in fk.FieldsForeignKey)
                    {
                        var localColumn = table.Columns.FirstOrDefault(c => c.Id == field.ColumnId);
                        var refColumn = referencedTable.Columns.FirstOrDefault(c => c.Id == field.ReferencedColumnId);

                        if (localColumn == null || refColumn == null)
                            invalidFields.Add(field);
                    }

                    // Remover campos órfãos
                    foreach (var field in invalidFields)
                        fk.FieldsForeignKey.Remove(field);

                    // Se FK ficou vazia, remover
                    if (fk.FieldsForeignKey.Count == 0)
                        invalidFks.Add(fk);
                }

                // Remover FKs inválidas
                foreach (var fk in invalidFks)
                    table.Fks.Remove(fk);
            }
        }
    }
}
```

#### Métodos Esperados

| Método | Parâmetros | Retorno | Descrição |
|--------|-----------|---------|-----------|
| `ValidateName(name)` | string | bool | Valida nomenclatura (whitelist + sem reservadas) |
| `IsNameReservedWord(name)` | string | bool | Verifica se é palavra reservada SQL |
| `CheckColumnUsageInIndexes(table, column)` | Table, Column | List<Index> | Retorna índices que usam coluna |
| `CheckColumnUsageInForeignKeys(table, column)` | Table, Column | List<ForeignKey> | Retorna FKs que usam coluna |
| `ValidateForeignKeyMapping(col1, col2)` | Column, Column | bool | Verifica compatibilidade de tipos |
| `ValidateForeignKeyConsistency(project)` | Project | void | Remove FKs órfãs e campos inválidos |

---

### 3. `JsonSerializationService.cs`

**Responsabilidade:** Serializar/desserializar `Project` para/de JSON com custom converters.

#### Implementação Básica

```csharp
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriptor.Models;

namespace Scriptor.Services
{
    public class JsonSerializationService
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[]
            {
                new GuidConverter(),
                new IndexColumnsConverter()
            }
        };

        public static string SerializeToJson(Project project)
        {
            return JsonConvert.SerializeObject(project, _settings);
        }

        public static void SerializeToFile(Project project, string filePath)
        {
            var json = SerializeToJson(project);
            File.WriteAllText(filePath, json);
        }

        public static Project DeserializeFromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<Project>(jsonString, _settings);
        }

        public static Project DeserializeFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

            var json = File.ReadAllText(filePath);
            return DeserializeFromJson(json);
        }
    }

    /// <summary>
    /// Converter customizado para Guid
    /// Serializa como string, desserializa de string para Guid
    /// </summary>
    public class GuidConverter : JsonConverter<Guid>
    {
        public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return Guid.Empty;

            if (Guid.TryParse(reader.Value.ToString(), out var guid))
                return guid;

            throw new JsonSerializationException($"Valor inválido para Guid: {reader.Value}");
        }

        public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    /// <summary>
    /// Converter customizado para List<Column> em Index
    /// Serializa apenas IDs das colunas (evita referências circulares)
    /// Desserialização requer reconstitução de referências (fazer em ProjectService)
    /// </summary>
    public class IndexColumnsConverter : JsonConverter<List<Column>>
    {
        public override List<Column> ReadJson(JsonReader reader, Type objectType, List<Column> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new List<Column>();

            var jArray = JArray.Load(reader);
            var columns = new List<Column>();

            foreach (var item in jArray)
            {
                // Na desserialização, receber apenas IDs (será reconstituído depois)
                if (item.Type == JTokenType.String)
                {
                    // Se for string (ID), criar Column com ID apenas
                    if (Guid.TryParse(item.Value<string>(), out var id))
                    {
                        columns.Add(new Column { Id = id });
                    }
                }
                else if (item.Type == JTokenType.Object)
                {
                    // Se for objeto, desserializar como Column normal
                    var column = item.ToObject<Column>();
                    columns.Add(column);
                }
            }

            return columns;
        }

        public override void WriteJson(JsonWriter writer, List<Column> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            if (value != null)
            {
                foreach (var column in value)
                {
                    writer.WriteValue(column.Id.ToString());
                }
            }

            writer.WriteEndArray();
        }
    }
}
```

#### Métodos Esperados

| Método | Parâmetros | Retorno | Descrição |
|--------|-----------|---------|-----------|
| `SerializeToJson(project)` | `Project` | string | Converte para JSON string |
| `SerializeToFile(project, filePath)` | Project, string | void | Salva em arquivo |
| `DeserializeFromJson(jsonString)` | string | `Project` | Converte de JSON string |
| `DeserializeFromFile(filePath)` | string | `Project` | Carrega de arquivo |

#### Custom Converters

**GuidConverter:** Serializa `Guid` como string (padrão JSON não suporta Guid nativamente)

**IndexColumnsConverter:** 
- **Serializa:** `Index.Columns` como array de strings (apenas IDs)
- **Desserializa:** Array de strings de volta para List<Column>
- Evita referências circulares e reduz tamanho do JSON

---

## 📁 Estrutura de Arquivos a Criar

```
Scriptor/Services/
├── ProjectService.cs
├── ValidationService.cs
└── JsonSerializationService.cs
```

---

## 📋 Criar Arquivo: `Utilities/Constants.cs`

```csharp
using System.Collections.Generic;

namespace Scriptor.Utilities
{
    public static class Constants
    {
        // Palavras reservadas SQL (SQL Server + Oracle)
        public static readonly HashSet<string> RESERVED_WORDS = new HashSet<string>
        {
            // DDL
            "ADD", "ALTER", "CREATE", "DROP", "RENAME", "TRUNCATE",
            // DML
            "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE",
            // Clausulas
            "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE",
            "ORDER", "BY", "GROUP", "HAVING", "UNION", "DISTINCT",
            // Joins
            "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON",
            // Outros
            "TABLE", "INDEX", "VIEW", "PROCEDURE", "FUNCTION",
            "PRIMARY", "KEY", "FOREIGN", "UNIQUE", "DEFAULT",
            "NULL", "CONSTRAINT", "CHECK", "CASE", "WHEN", "THEN", "ELSE"
        };

        public static readonly char[] INVALID_NAME_CHARACTERS = new[]
        {
            ' ', '@', '#', '$', '%', '&', '*', '(', ')', '-', '+', '=',
            '[', ']', '{', '}', '|', '\\', ':', ';', '\"', '\'', '<', '>', ',', '.', '/'
        };

        // Tamanhos padrão
        public const int DEFAULT_STRING_SIZE = 255;
        public const int DEFAULT_DECIMAL_SIZE = 10;
        public const int DEFAULT_DECIMAL_PRECISION = 2;
    }
}
```

---

## 🔧 Checklist de Implementação

### Pré-Requisitos
- [ ] Newtonsoft.Json instalado via NuGet (dotnet add package)
- [ ] Pasta `Scriptor/Services/` criada
- [ ] Referência a `Scriptor.Models` adicionada nos services

### ProjectService
- [ ] Singleton com `GetInstance()`
- [ ] Propriedade `_currentProject` privada
- [ ] Método `GetCurrentProject()` com lazy initialization
- [ ] Método `CreateNewProject(clientName, projectName)`
- [ ] Método `AddTable(table)`
- [ ] Método `RemoveTable(tableId)`
- [ ] Método `GetTableById(tableId)`
- [ ] Método `UpdateTable(table)`
- [ ] Método `SaveProject(filePath)` — delega a JsonSerializationService
- [ ] Método `LoadProject(filePath)` — delega a JsonSerializationService

### ValidationService
- [ ] Método `ValidateName(name)` — Regex + reservadas
- [ ] Método `IsNameReservedWord(name)`
- [ ] Método `CheckColumnUsageInIndexes(table, column)` — Retorna List<Index>
- [ ] Método `CheckColumnUsageInForeignKeys(table, column)` — Retorna List<ForeignKey>
- [ ] Método `ValidateForeignKeyMapping(col1, col2)` — Verifica tipos
- [ ] Método `ValidateForeignKeyConsistency(project)` — Remove órfãs

### JsonSerializationService
- [ ] Método `SerializeToJson(project)` → string
- [ ] Método `SerializeToFile(project, filePath)` → arquivo
- [ ] Método `DeserializeFromJson(jsonString)` → Project
- [ ] Método `DeserializeFromFile(filePath)` → Project
- [ ] Custom Converter: `GuidConverter`
- [ ] Custom Converter: `IndexColumnsConverter`

### Constants.cs
- [ ] HashSet `RESERVED_WORDS` preenchido
- [ ] Array `INVALID_NAME_CHARACTERS` preenchido
- [ ] Constantes de tamanho padrão

---

## ✅ Verificação e Testes

### Teste 1: ProjectService Singleton
```csharp
// Test code
var service1 = ProjectService.GetInstance();
var service2 = ProjectService.GetInstance();

Assert.AreSame(service1, service2);  // Mesma instância?
```

### Teste 2: Criar e Obter Projeto
```csharp
var service = ProjectService.GetInstance();
service.CreateNewProject("Acme", "CRM");

var project = service.GetCurrentProject();
Assert.AreEqual("Acme", project.NameClient);
Assert.AreEqual("CRM", project.NameProject);
```

### Teste 3: Validação de Nome
```csharp
Assert.IsTrue(ValidationService.ValidateName("User_Table"));
Assert.IsFalse(ValidationService.ValidateName("User Table"));  // Espaço
Assert.IsFalse(ValidationService.ValidateName("SELECT"));      // Reservada
```

### Teste 4: Serialização JSON
```csharp
var project = new Project 
{ 
    NameClient = "Acme", 
    NameProject = "CRM" 
};

var json = JsonSerializationService.SerializeToJson(project);
var restored = JsonSerializationService.DeserializeFromJson(json);

Assert.AreEqual(project.NameClient, restored.NameClient);
```

### Teste 5: Serialização em Arquivo
```csharp
var project = new Project { NameClient = "Test", NameProject = "Test" };
var filePath = "test_project.json";

JsonSerializationService.SerializeToFile(project, filePath);
var loaded = JsonSerializationService.DeserializeFromFile(filePath);

Assert.AreEqual(project.NameProject, loaded.NameProject);
File.Delete(filePath);  // Cleanup
```

---

## 🔍 Arquivo JSON Esperado

```json
{
  "NameClient": "Acme Corp",
  "NameProject": "CRM System",
  "Tables": [
    {
      "Id": "550e8400-e29b-41d4-a716-446655440000",
      "Name": "Users",
      "Description": "Usuários do sistema",
      "Columns": [
        {
          "Id": "550e8400-e29b-41d4-a716-446655440001",
          "Name": "Id",
          "DataType": "INTEGER",
          "Size": null,
          "Precision": null,
          "IsPrimaryKey": true,
          "AllowNull": false
        }
      ],
      "Indexes": [
        {
          "Name": "IDX_Users_Email",
          "Columns": ["550e8400-e29b-41d4-a716-446655440002"]
        }
      ],
      "Fks": []
    }
  ]
}
```

**Observação:** `Columns` em `Indexes` é serializado como array de string (apenas IDs).

---

## 📝 Notas Importantes

### Inicialização do Singleton
Primeiro acesso a `ProjectService.GetInstance()` cria nova instância.

### JSON Indentado
A configuração `Formatting = Formatting.Indented` torna o JSON legível (importante para debugging).

### GUIDs em JSON
GUIDs não têm tipo nativo em JSON, por isso o converter converte para string.

### Cascata de Exclusão (Será Implementada na Fase 8)
`ValidateForeignKeyConsistency` remove FKs órfãs automaticamente ao desserializar.

---

## 🔗 Próximos Passos

Após concluir Fase 2:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 3: Mapeamento de Tipos** (`03_phase_mappers.md`)

**Tempo Estimado:** 1-1.5 horas

**Verificação Final:**
- [ ] Compilar sem erros
- [ ] 5 testes acima executados com sucesso
- [ ] JSON salvo e carregado corretamente
