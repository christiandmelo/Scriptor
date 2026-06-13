# Visão Geral: Gerador de Scripts SQL WinForms

## 🎯 Objetivo do Projeto

Ferramenta visual que permite aos usuários modelar banco de dados relacionais através de interface WinForms e gerar automaticamente scripts DDL (Data Definition Language) para **SQL Server** e **Oracle**.

### Benefícios
- ✅ Modelagem visual intuitiva (não precisa saber DDL)
- ✅ Suporte a dois SGBDs com um único modelo
- ✅ Persistência local em JSON (sem servidor)
- ✅ Suporte a relacionamentos complexos (FK compostas)

---

## 🏗️ Stack Tecnológico

| Componente | Tecnologia |
|-----------|-----------|
| Linguagem | C# (.NET 8.0+) |
| Interface | Windows Forms (WinForms) |
| Persistência | Arquivo JSON local |
| Serialização | Newtonsoft.Json (NuGet) |
| Padrões | MVP + Strategy Pattern |

---

## 📊 Decisões Arquiteturais (Confirmadas)

### 1️⃣ Gerenciamento de Estado: `ProjectService` Singleton
- **O quê:** Uma única instância de `ProjectService` gerencia todo o estado da aplicação
- **Por quê:** Single Source of Truth — evita inconsistências entre UI e dados
- **Como:** Inicializado em `Program.Main()`, injetado nos Forms

### 2️⃣ Persistência: Newtonsoft.Json + Custom Converters
- **O quê:** Serializar/desserializar `Project` para arquivo JSON com converters customizados
- **Por quê:** GUIDs, Listas e referências circulares requerem tratamento especial
- **Como:** `JsonSerializationService` com `JsonConverter` para Guid, Index.Columns, etc.

### 3️⃣ Tradução de Tipos: `DataTypeMapper` (Camada Dedicada)
- **O quê:** Mapear tipos neutros (STRING, INTEGER, DATE) para SQL Server/Oracle específicos
- **Por quê:** Desacoplar UI de dialetos SQL, facilitar adição de novos SGBDs
- **Como:** `DataTypeMapper.MapToSqlServer()` e `MapToOracle()` estáticos

---

## 📁 Escopo: Incluído vs Excluído

### ✅ Incluído no Projeto
- Modelos de domínio (Project, Table, Column, Index, ForeignKey)
- Persistência JSON com validação
- Validações de nomenclatura (nomes reservados, caracteres especiais)
- Mapeamento de tipos de dados
- Geradores DDL (SQL Server + Oracle) via Strategy Pattern
- 3 Telas WinForms (Principal, Editor de Tabelas, Modal FK)
- Cascata de exclusão (remover coluna → atualiza índices/FKs)
- Suporte a auto-relacionamento (FK para mesma tabela)
- Suporte a FK composta (múltiplas colunas)

### ❌ Excluído (Fora do Escopo)
- ❌ Edição de dados (DML — INSERT, UPDATE, DELETE)
- ❌ Visualização gráfica de relacionamentos (ER Diagram)
- ❌ Exportação em outros formatos (XML, PDF)
- ❌ Versionamento/histórico de projetos
- ❌ Suporte a PostgreSQL, MySQL, etc. (mas arquitetura permite extensão)

---

## 🚀 Fases de Implementação (9 Total)

Cada fase é **sequencial** e depende da anterior. Após cada fase, compilar e validar.

| # | Fase | Dependências | Status |
|---|------|--------------|--------|
| 1 | Modelos Base | — | ⏳ Não iniciado |
| 2 | Serviços & Persistência | 1 | ⏳ Não iniciado |
| 3 | Mapeamento de Tipos | 1 | ⏳ Não iniciado |
| 4 | Geradores SQL | 2, 3 | ⏳ Não iniciado |
| 5 | UI Principal (MainForm) | 2 | ⏳ Não iniciado |
| 6 | Editor de Tabelas | 5 | ⏳ Não iniciado |
| 7 | Modal FK | 6 | ⏳ Não iniciado |
| 8 | Validações & Cascata | 6, 7 | ⏳ Não iniciado |
| 9 | Testes & Integração | Todas | ⏳ Não iniciado |

---

## 📦 Estrutura de Pastas (Inicial)

```
d:\Projetos\Scriptor\
├── Scriptor/                        # Projeto C# principal
│   ├── Models/                      # 6 classes de domínio
│   │   ├── Project.cs
│   │   ├── Table.cs
│   │   ├── Column.cs
│   │   ├── Index.cs
│   │   ├── ForeignKey.cs
│   │   └── FieldsForeignKey.cs
│   │
│   ├── Services/                    # 3 serviços
│   │   ├── ProjectService.cs
│   │   ├── JsonSerializationService.cs
│   │   └── ValidationService.cs
│   │
│   ├── Mappers/                     # Mapeadores
│   │   └── DataTypeMapper.cs
│   │
│   ├── Generators/                  # Geradores SQL
│   │   ├── IScriptGenerator.cs
│   │   ├── SqlServerGenerator.cs
│   │   └── OracleGenerator.cs
│   │
│   ├── UI/                          # 3 Forms
│   │   ├── MainForm.cs
│   │   ├── MainForm.Designer.cs
│   │   ├── TableEditorForm.cs
│   │   ├── TableEditorForm.Designer.cs
│   │   ├── ForeignKeyModalForm.cs
│   │   └── ForeignKeyModalForm.Designer.cs
│   │
│   ├── Utilities/                   # Constantes e helpers
│   │   ├── Constants.cs
│   │   └── NeutralDataType.cs
│   │
│   ├── Main.cs                      # Form principal (ou refactor)
│   ├── Main.Designer.cs
│   ├── Program.cs                   # Ponto de entrada (modificar)
│   ├── Scriptor.csproj              # Projeto (adicionar Newtonsoft.Json)
│   ├── Scriptor.sln
│   └── bin/, obj/                   # Build output
│
├── docs/                            # 📚 Documentação
│   ├── planning/                    # 9 fases detalhadas
│   │   ├── 00_overview.md           (Este arquivo)
│   │   ├── 01_phase_models.md
│   │   ├── 02_phase_services.md
│   │   ├── 03_phase_mappers.md
│   │   ├── 04_phase_generators.md
│   │   ├── 05_phase_mainform.md
│   │   ├── 06_phase_tableeditor.md
│   │   ├── 07_phase_fkmodal.md
│   │   ├── 08_phase_validations.md
│   │   └── 09_phase_testing.md
│   │
│   └── architecture/                # Arquitetura e decisões
│       ├── architecture.md
│       └── decisions.md
│
└── PLANNING.md                      # 📄 Índice principal (raiz)
```

---

## ⚙️ Workflow Recomendado

### Por Fase
1. 📖 Ler documento da fase (ex: `01_phase_models.md`)
2. 🏗️ Criar arquivos conforme especificação
3. 📝 Implementar código
4. ✅ Compilar e validar (sem erros)
5. 🧪 Testar conforme checklist da fase
6. ✔️ Marcar como "Concluída" em `PLANNING.md`
7. ➡️ Passar para próxima fase

### Checklist Geral (Antes de Começar)
- [ ] Newtonsoft.Json adicionado via NuGet
- [ ] Pasta `Models/` criada
- [ ] Pasta `Services/` criada
- [ ] Pasta `Mappers/` criada
- [ ] Pasta `Generators/` criada
- [ ] Pasta `UI/` criada
- [ ] Pasta `Utilities/` criada
- [ ] Pasta `docs/` criada com subpastas

---

## 📞 FAQ

### P: Posso fazer as fases em paralelo?
**R:** Não. Há dependências. Fase 2 depende de Fase 1, Fase 4 depende de 2 e 3, etc. Siga a ordem sequencial.

### P: Posso pular uma fase?
**R:** Não. Cada fase fornece fundação para a próxima. Pular causará erros e retrabalho.

### P: E se encontrar um bug na Fase anterior?
**R:** Fixe imediatamente. Não passe para próxima fase com bugs conhecidos.

### P: Preciso escrever testes unitários?
**R:** Fase 9 cobre testes manuais (end-to-end). Testes unitários são opcionais, mas recomendados para `Services/` e `Generators/`.

### P: Quantas linhas de código será?
**R:** Aproximadamente 2000-3000 linhas (Models ~300, Services ~400, Generators ~600, UI Forms ~1200, Utilities ~50, Validators ~150).

---

## 🔄 Atualização de Status

**Estado Atual:** Plano definido, pronto para Fase 1  
**Última Atualização:** 2026-06-13  
**Próximo Passo:** Iniciar Fase 1 — Criar Modelos Base
