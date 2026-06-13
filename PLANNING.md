# Plano de Implementação: Gerador de Scripts SQL WinForms

## 📋 Índice de Documentação

- **[00 - Visão Geral](docs/planning/00_overview.md)** — Resumo executivo, decisões arquiteturais, escopo
- **[Arquitetura](docs/architecture/architecture.md)** — Estrutura de pastas, diagrama de componentes, padrões
- **[Decisões Arquiteturais](docs/architecture/decisions.md)** — Justificativas e trade-offs

### Fases de Implementação

1. **[Fase 1 - Modelos Base](docs/planning/01_phase_models.md)** — Classes de domínio (Project, Table, Column, Index, FK)
2. **[Fase 2 - Serviços & Persistência](docs/planning/02_phase_services.md)** — ProjectService, JSON, Validações
3. **[Fase 3 - Mapeamento de Tipos](docs/planning/03_phase_mappers.md)** — DataTypeMapper, Tradução SQL
4. **[Fase 4 - Geradores SQL](docs/planning/04_phase_generators.md)** — Strategy Pattern, SqlServer & Oracle
5. **[Fase 5 - UI Principal](docs/planning/05_phase_mainform.md)** — MainForm, Menu, DataGridView
6. **[Fase 6 - Editor de Tabelas](docs/planning/06_phase_tableeditor.md)** — TabControl, 3 Abas, Gerenciamento
7. **[Fase 7 - Modal FK](docs/planning/07_phase_fkmodal.md)** — Chaves Estrangeiras Compostas
8. **[Fase 8 - Validações & Cascata](docs/planning/08_phase_validations.md)** — Lógica de Exclusão, Integridade
9. **[Fase 9 - Testes & Integração](docs/planning/09_phase_testing.md)** — Testes E2E, Validações

---

## 🎯 Quick Start

### Resumo em 60 Segundos
- **Objetivo:** Ferramenta visual para gerar DDL (CREATE TABLE, INDEX, FK) para SQL Server e Oracle
- **Stack:** C# .NET, WinForms, JSON local, Newtonsoft.Json
- **Arquitetura:** MVP + Strategy Pattern
- **Decisões:** Singleton ProjectService, Custom JSON Converters, DataTypeMapper
- **9 Fases Sequenciais:** Modelos → Serviços → Mapeadores → Geradores → UI Principal → Tabelas → FK → Validações → Testes

### Estrutura de Pastas
```
Scriptor/
├── Scriptor/
│   ├── Models/                  # 6 classes de domínio
│   ├── Services/                # 3 serviços de negócio
│   ├── Mappers/                 # Mapeadores de tipos
│   ├── Generators/              # Geradores SQL (Strategy)
│   ├── UI/                       # 3 Forms
│   └── Utilities/               # Constantes, enums
├── docs/
│   ├── planning/                # 9 fases detalhadas
│   └── architecture/            # Arquitetura e decisões
└── tests/                        # (Opcional) Testes unitários
```

---

## ✅ Status de Implementação

- [x] Fase 1: Modelos Base
- [x] Fase 2: Serviços & Persistência
- [ ] Fase 3: Mapeamento de Tipos
- [ ] Fase 4: Geradores SQL
- [ ] Fase 5: UI Principal
- [ ] Fase 6: Editor de Tabelas
- [ ] Fase 7: Modal FK
- [ ] Fase 8: Validações & Cascata
- [ ] Fase 9: Testes & Integração

---

## 🔗 Links Úteis

- **Newtonsoft.Json Docs:** https://www.newtonsoft.com/json
- **WinForms Best Practices:** https://docs.microsoft.com/en-us/dotnet/desktop/winforms/overview
- **C# Async/Await:** https://docs.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- **SQL Server T-SQL DDL:** https://docs.microsoft.com/en-us/sql/t-sql/statements/statements
- **Oracle SQL DDL:** https://docs.oracle.com/en/database/

---

## 📞 Suporte

Para dúvidas durante a implementação, consulte:
1. Arquivo específico da fase (ex: `01_phase_models.md`)
2. Arquivo de arquitetura (`architecture.md`)
3. Decisões (`decisions.md`)

**Última atualização:** 2026-06-13
