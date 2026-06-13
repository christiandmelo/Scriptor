# Fase 3: Mapeamento de Tipos de Dados

## 📌 Objetivo

Implementar a camada `DataTypeMapper` que traduz tipos de dados **neutros** (STRING, INTEGER, DECIMAL, DATE, BOOLEAN) para dialetos específicos (SQL Server, Oracle).

**Dependências:** Fase 1 (Modelos)  
**Status:** ⏳ Não iniciado

---

## 📐 Implementações

### 1. `Utilities/NeutralDataType.cs`

Enum com tipos de dados suportados pela aplicação.

```csharp
namespace Scriptor.Utilities
{
    public enum NeutralDataType
    {
        String,
        Integer,
        Decimal,
        Date,
        Boolean,
        Text,
        Blob
    }
}
```

**Tipos Suportados:**
- `String` — VARCHAR (tamanho variável)
- `Integer` — INT (número inteiro)
- `Decimal` — DECIMAL/NUMBER (ponto flutuante)
- `Date` — DATE/DATETIME
- `Boolean` — BOOLEAN/BIT/CHAR(1)
- `Text` — TEXT/CLOB (texto grande)
- `Blob` — BLOB/BYTEA (dados binários)

---

### 2. `Mappers/DataTypeMapper.cs`

Responsável por mapear tipos neutros para SQL específicos.

```csharp
using System;
using Scriptor.Utilities;

namespace Scriptor.Mappers
{
    public static class DataTypeMapper
    {
        /// <summary>
        /// Mapeia tipo neutro para SQL Server
        /// </summary>
        public static string MapToSqlServer(string neutralType, int? size, int? precision)
        {
            if (string.IsNullOrWhiteSpace(neutralType))
                throw new ArgumentException("Tipo de dado não pode estar vazio");

            return neutralType.ToUpper() switch
            {
                "STRING" => $"VARCHAR({size ?? Constants.DEFAULT_STRING_SIZE})",
                "INTEGER" => "INT",
                "DECIMAL" => $"DECIMAL({size ?? Constants.DEFAULT_DECIMAL_SIZE},{precision ?? Constants.DEFAULT_DECIMAL_PRECISION})",
                "DATE" => "DATETIME",
                "BOOLEAN" => "BIT",
                "TEXT" => "TEXT",
                "BLOB" => "VARBINARY(MAX)",
                _ => throw new ArgumentException($"Tipo de dado não suportado: {neutralType}")
            };
        }

        /// <summary>
        /// Mapeia tipo neutro para Oracle
        /// </summary>
        public static string MapToOracle(string neutralType, int? size, int? precision)
        {
            if (string.IsNullOrWhiteSpace(neutralType))
                throw new ArgumentException("Tipo de dado não pode estar vazio");

            return neutralType.ToUpper() switch
            {
                "STRING" => $"VARCHAR2({size ?? Constants.DEFAULT_STRING_SIZE})",
                "INTEGER" => "NUMBER(10)",
                "DECIMAL" => $"DECIMAL({size ?? Constants.DEFAULT_DECIMAL_SIZE},{precision ?? Constants.DEFAULT_DECIMAL_PRECISION})",
                "DATE" => "DATE",
                "BOOLEAN" => "CHAR(1)",
                "TEXT" => "CLOB",
                "BLOB" => "BLOB",
                _ => throw new ArgumentException($"Tipo de dado não suportado: {neutralType}")
            };
        }

        /// <summary>
        /// Retorna tamanho padrão para um tipo (se não especificado)
        /// </summary>
        public static int GetDefaultSize(string neutralType)
        {
            return neutralType.ToUpper() switch
            {
                "STRING" => Constants.DEFAULT_STRING_SIZE,
                "DECIMAL" => Constants.DEFAULT_DECIMAL_SIZE,
                "INTEGER" => 0,  // INT não usa tamanho
                "DATE" => 0,
                "BOOLEAN" => 0,
                "TEXT" => 0,
                "BLOB" => 0,
                _ => throw new ArgumentException($"Tipo desconhecido: {neutralType}")
            };
        }

        /// <summary>
        /// Retorna precisão padrão para DECIMAL (casas decimais)
        /// </summary>
        public static int GetDefaultPrecision(string neutralType)
        {
            return neutralType.ToUpper() switch
            {
                "DECIMAL" => Constants.DEFAULT_DECIMAL_PRECISION,
                _ => 0
            };
        }

        /// <summary>
        /// Verifica se um tipo específico de SQL Server é válido
        /// </summary>
        public static bool IsValidTypeForSqlServer(string sqlServerType)
        {
            var validTypes = new[]
            {
                "VARCHAR", "INT", "DECIMAL", "DATETIME", "BIT", "TEXT", "VARBINARY",
                "NVARCHAR", "FLOAT", "REAL", "NUMERIC", "DATE", "TIME", "SMALLINT", "BIGINT"
            };

            return Array.Exists(validTypes, t => t.Equals(sqlServerType, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Verifica se um tipo específico de Oracle é válido
        /// </summary>
        public static bool IsValidTypeForOracle(string oracleType)
        {
            var validTypes = new[]
            {
                "VARCHAR2", "NUMBER", "DECIMAL", "DATE", "CHAR", "CLOB", "BLOB",
                "FLOAT", "INTEGER", "SMALLINT", "TIMESTAMP"
            };

            return Array.Exists(validTypes, t => t.Equals(oracleType, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Converte tipo neutro de string para enum
        /// </summary>
        public static NeutralDataType ParseNeutralType(string type)
        {
            return type.ToUpper() switch
            {
                "STRING" => NeutralDataType.String,
                "INTEGER" => NeutralDataType.Integer,
                "DECIMAL" => NeutralDataType.Decimal,
                "DATE" => NeutralDataType.Date,
                "BOOLEAN" => NeutralDataType.Boolean,
                "TEXT" => NeutralDataType.Text,
                "BLOB" => NeutralDataType.Blob,
                _ => throw new ArgumentException($"Tipo desconhecido: {type}")
            };
        }
    }
}
```

#### Métodos Esperados

| Método | Parâmetros | Retorno | Exemplo |
|--------|-----------|---------|---------|
| `MapToSqlServer(type, size, precision)` | string, int?, int? | string | `"STRING", 100, null` → `"VARCHAR(100)"` |
| `MapToOracle(type, size, precision)` | string, int?, int? | string | `"STRING", 100, null` → `"VARCHAR2(100)"` |
| `GetDefaultSize(type)` | string | int | `"STRING"` → `255` |
| `GetDefaultPrecision(type)` | string | int | `"DECIMAL"` → `2` |
| `IsValidTypeForSqlServer(type)` | string | bool | `"VARCHAR"` → `true` |
| `IsValidTypeForOracle(type)` | string | bool | `"VARCHAR2"` → `true` |
| `ParseNeutralType(type)` | string | NeutralDataType | `"STRING"` → `NeutralDataType.String` |

---

## 📊 Tabela de Mapeamentos

### SQL Server

| Neutro | SQL Server | Com Tamanho |
|--------|-----------|-----------|
| STRING | VARCHAR | VARCHAR(255) |
| INTEGER | INT | INT |
| DECIMAL | DECIMAL | DECIMAL(10,2) |
| DATE | DATETIME | DATETIME |
| BOOLEAN | BIT | BIT |
| TEXT | TEXT | TEXT |
| BLOB | VARBINARY(MAX) | VARBINARY(MAX) |

### Oracle

| Neutro | Oracle | Com Tamanho |
|--------|--------|-----------|
| STRING | VARCHAR2 | VARCHAR2(255) |
| INTEGER | NUMBER(10) | NUMBER(10) |
| DECIMAL | DECIMAL | DECIMAL(10,2) |
| DATE | DATE | DATE |
| BOOLEAN | CHAR(1) | CHAR(1) |
| TEXT | CLOB | CLOB |
| BLOB | BLOB | BLOB |

---

## 📁 Estrutura de Arquivos a Criar/Modificar

```
Scriptor/Utilities/
├── Constants.cs          (já criado)
├── NeutralDataType.cs    (novo - enum)
└── ...

Scriptor/Mappers/
└── DataTypeMapper.cs     (novo - mapper)
```

---

## 🔧 Checklist de Implementação

### NeutralDataType.cs
- [ ] Enum `NeutralDataType` com 7 valores
- [ ] Valores: String, Integer, Decimal, Date, Boolean, Text, Blob

### DataTypeMapper.cs
- [ ] Método `MapToSqlServer()` com tratamento de todos os 7 tipos
- [ ] Método `MapToOracle()` com tratamento de todos os 7 tipos
- [ ] Método `GetDefaultSize()` retorna tamanho padrão
- [ ] Método `GetDefaultPrecision()` retorna precisão padrão
- [ ] Método `IsValidTypeForSqlServer()` valida tipo SQL Server
- [ ] Método `IsValidTypeForOracle()` valida tipo Oracle
- [ ] Método `ParseNeutralType()` converte string para enum
- [ ] Todos métodos são `static`
- [ ] Tratamento de exceções (ArgumentException) para tipos inválidos

---

## ✅ Verificação e Testes

### Teste 1: Mapeamento STRING
```csharp
var sqlServer = DataTypeMapper.MapToSqlServer("STRING", 100, null);
var oracle = DataTypeMapper.MapToOracle("STRING", 100, null);

Assert.AreEqual("VARCHAR(100)", sqlServer);
Assert.AreEqual("VARCHAR2(100)", oracle);
```

### Teste 2: Mapeamento DECIMAL
```csharp
var sqlServer = DataTypeMapper.MapToSqlServer("DECIMAL", 10, 2);
var oracle = DataTypeMapper.MapToOracle("DECIMAL", 10, 2);

Assert.AreEqual("DECIMAL(10,2)", sqlServer);
Assert.AreEqual("DECIMAL(10,2)", oracle);
```

### Teste 3: Tamanho Padrão
```csharp
var defaultStringSize = DataTypeMapper.GetDefaultSize("STRING");
var defaultDecimalSize = DataTypeMapper.GetDefaultSize("DECIMAL");

Assert.AreEqual(255, defaultStringSize);
Assert.AreEqual(10, defaultDecimalSize);
```

### Teste 4: STRING sem Tamanho
```csharp
var sqlServer = DataTypeMapper.MapToSqlServer("STRING", null, null);
var oracle = DataTypeMapper.MapToOracle("STRING", null, null);

Assert.AreEqual("VARCHAR(255)", sqlServer);  // Usa padrão
Assert.AreEqual("VARCHAR2(255)", oracle);   // Usa padrão
```

### Teste 5: Validação de Tipo
```csharp
Assert.IsTrue(DataTypeMapper.IsValidTypeForSqlServer("VARCHAR"));
Assert.IsTrue(DataTypeMapper.IsValidTypeForOracle("VARCHAR2"));
Assert.IsFalse(DataTypeMapper.IsValidTypeForSqlServer("INVALID_TYPE"));
```

### Teste 6: Parse Neutro para Enum
```csharp
var neutralType = DataTypeMapper.ParseNeutralType("STRING");
Assert.AreEqual(NeutralDataType.String, neutralType);
```

---

## 📝 Notas Importantes

### Tipos Neutros
Os tipos neutros são **agnósticos** ao SGBD. Permitem que o usuário trabalhe sem conhecer detalhes do SQL específico.

### Tamanhos Padrão
Se o usuário não especificar tamanho para STRING, usa DEFAULT_STRING_SIZE (255).

### DECIMAL vs NUMBER
- **SQL Server:** `DECIMAL(10, 2)`
- **Oracle:** `DECIMAL(10, 2)` (também suporta `NUMBER(10, 2)`)

### BOOLEAN
- **SQL Server:** `BIT` (0 = false, 1 = true)
- **Oracle:** `CHAR(1)` ('N' = false, 'Y' = true)

### TEXT e BLOB
- **TEXT:** Para dados textuais grandes (> 8000 caracteres)
- **BLOB:** Para dados binários arbitrários

---

## 🔗 Próximos Passos

Após concluir Fase 3:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 4: Geradores SQL** (`04_phase_generators.md`)

**Tempo Estimado:** 30-45 minutos

**Verificação Final:**
- [ ] Compilar sem erros
- [ ] 6 testes acima executados com sucesso
- [ ] Todos os mapeamentos funcionam (STRING, INTEGER, DECIMAL, DATE, BOOLEAN, TEXT, BLOB)
