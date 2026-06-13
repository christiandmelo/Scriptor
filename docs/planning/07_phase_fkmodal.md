# Fase 7: Interface Gráfica - Modal de Chave Estrangeira

## 📌 Objetivo

Implementar modal para configurar Foreign Keys compostas, com validação de tipos e suporte a múltiplos pares de colunas.

**Dependências:** Fase 6 (TableEditorForm)  
**Status:** ⏳ Não iniciado

---

## 🎨 Layout

```
┌────────────────────────────────────────────────────────┐
│ Configurar Chave Estrangeira                      [X] │
├────────────────────────────────────────────────────────┤
│                                                        │
│ Tabela de Referência:  [▼ Users    ▼]                │
│                                                        │
│ Mapeamento de Colunas:                                │
│ ┌──────────────────────────────────────────────────┐ │
│ │ Campo Local │ Campo Referência  │                │ │
│ ├──────────────────────────────────────────────────┤ │
│ │ UserId ▼    │ Users.Id ▼       │               │ │
│ │ CompanyId ▼ │ Users.CompanyId ▼│               │ │
│ └──────────────────────────────────────────────────┘ │
│                                                        │
│ [Adicionar Par] [Remover Par]                        │
│                                                        │
│ [Salvar FK] [Cancelar]                               │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## 🔧 Componentes

### Parte Superior
- **Label:** "Tabela de Referência:"
- **ComboBox:** Lista de todas as tabelas do projeto

### DataGridView de Mapeamento
Dois tipos de colunas:

| Coluna | Tipo | Conteúdo |
|--------|------|---------|
| Campo Local | ComboBox | Colunas da tabela atual |
| Campo Referência | ComboBox | Colunas da tabela referenciada (muda com seleção do ComboBox superior) |

### Botões
- **Adicionar Par:** Insere nova linha para mapeamento
- **Remover Par:** Remove par selecionado
- **Salvar FK:** Valida e retorna `ForeignKey`
- **Cancelar:** Descarta mudanças

---

## 🔧 Validações

1. **Tabela deve existir:** Validar que tabela selecionada é válida
2. **Campos locais devem existir:** Coluna deve estar na tabela atual
3. **Campos referenciados devem existir:** Coluna deve estar na tabela selecionada
4. **Tipos compatíveis:** `ValidationService.ValidateForeignKeyMapping()` verifica tipos
5. **Sem pares vazios:** Ambos os campos devem estar preenchidos
6. **Chave composta:** Permitir múltiplos pares

---

## 💻 Pseudocódigo

```csharp
public partial class ForeignKeyModalForm : Form
{
    private Table _currentTable;  // Tabela atual (de onde vem a FK)
    private Project _project;     // Projeto inteiro (para listar tabelas e colunas)
    private ForeignKey _foreignKey;

    public ForeignKeyModalForm(Table currentTable, Project project, ForeignKey existingFK = null)
    {
        InitializeComponent();
        _currentTable = currentTable;
        _project = project;
        _foreignKey = existingFK ?? new ForeignKey();
    }

    private void ForeignKeyModalForm_Load(object sender, EventArgs e)
    {
        InitializeComboBoxes();
        InitializeDataGridView();
        RefreshUI();
    }

    private void InitializeComboBoxes()
    {
        // Preencher ComboBox de tabelas
        comboBoxReferencedTable.DataSource = _project.Tables.Select(t => t.Name).ToList();

        // Se FK existe, selecionar tabela referenciada
        if (_foreignKey.ReferencedTableId != Guid.Empty)
        {
            var refTable = _project.Tables.FirstOrDefault(t => t.Id == _foreignKey.ReferencedTableId);
            if (refTable != null)
                comboBoxReferencedTable.SelectedItem = refTable.Name;
        }
    }

    private void InitializeDataGridView()
    {
        // Coluna 1: Campo Local (ComboBox)
        var localColColumn = new DataGridViewComboBoxColumn
        {
            Name = "LocalColumn",
            HeaderText = "Campo Local",
            DataSource = _currentTable.Columns.Select(c => c.Name).ToList()
        };

        // Coluna 2: Campo Referência (ComboBox) — será populado dinamicamente
        var refColColumn = new DataGridViewComboBoxColumn
        {
            Name = "ReferencedColumn",
            HeaderText = "Campo Referência"
        };

        dataGridViewMapping.Columns.Add(localColColumn);
        dataGridViewMapping.Columns.Add(refColColumn);

        // Preencher com pares existentes
        if (_foreignKey.FieldsForeignKey != null)
        {
            foreach (var field in _foreignKey.FieldsForeignKey)
            {
                var localCol = _currentTable.Columns.FirstOrDefault(c => c.Id == field.ColumnId);
                var refCol = GetReferencedTable()?.Columns.FirstOrDefault(c => c.Id == field.ReferencedColumnId);

                if (localCol != null && refCol != null)
                {
                    dataGridViewMapping.Rows.Add(localCol.Name, refCol.Name);
                }
            }
        }
    }

    private Table GetReferencedTable()
    {
        var tableName = comboBoxReferencedTable.SelectedItem?.ToString();
        return _project.Tables.FirstOrDefault(t => t.Name == tableName);
    }

    private void ComboBoxReferencedTable_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Atualizar ComboBox de campos referenciados
        var refTable = GetReferencedTable();
        var refColNames = refTable?.Columns.Select(c => c.Name).ToList() ?? new List<string>();

        var refColumn = dataGridViewMapping.Columns["ReferencedColumn"] as DataGridViewComboBoxColumn;
        refColumn.DataSource = refColNames;
    }

    private void AddPair_Click(object sender, EventArgs e)
    {
        dataGridViewMapping.Rows.Add();
    }

    private void RemovePair_Click(object sender, EventArgs e)
    {
        if (dataGridViewMapping.SelectedRows.Count == 0)
            return;

        var index = dataGridViewMapping.SelectedRows[0].Index;
        dataGridViewMapping.Rows.RemoveAt(index);
    }

    private void SaveFK_Click(object sender, EventArgs e)
    {
        // Validar seleção de tabela
        var refTable = GetReferencedTable();
        if (refTable == null)
        {
            MessageBox.Show("Selecione uma tabela de referência");
            return;
        }

        // Validar pares
        if (dataGridViewMapping.Rows.Count == 0)
        {
            MessageBox.Show("Defina pelo menos um par de colunas");
            return;
        }

        var fieldsList = new List<FieldsForeignKey>();

        foreach (DataGridViewRow row in dataGridViewMapping.Rows)
        {
            var localColName = row.Cells["LocalColumn"].Value?.ToString();
            var refColName = row.Cells["ReferencedColumn"].Value?.ToString();

            if (string.IsNullOrEmpty(localColName) || string.IsNullOrEmpty(refColName))
            {
                MessageBox.Show("Todos os pares devem ter campo local e referência");
                return;
            }

            var localCol = _currentTable.Columns.FirstOrDefault(c => c.Name == localColName);
            var refCol = refTable.Columns.FirstOrDefault(c => c.Name == refColName);

            if (localCol == null || refCol == null)
            {
                MessageBox.Show($"Coluna inválida: {localColName} ou {refColName}");
                return;
            }

            // Validar tipos compatíveis
            if (!ValidationService.ValidateForeignKeyMapping(localCol, refCol))
            {
                MessageBox.Show($"Tipos incompatíveis: {localColName} ({localCol.DataType}) vs {refColName} ({refCol.DataType})");
                return;
            }

            fieldsList.Add(new FieldsForeignKey
            {
                ColumnId = localCol.Id,
                ReferencedColumnId = refCol.Id
            });
        }

        // Construir FK
        _foreignKey.ReferencedTableId = refTable.Id;
        _foreignKey.FieldsForeignKey = fieldsList;

        DialogResult = DialogResult.OK;
        Close();
    }

    public ForeignKey GetForeignKey()
    {
        return _foreignKey;
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
```

---

## 📁 Estrutura de Arquivos

```
Scriptor/UI/
├── ForeignKeyModalForm.cs
├── ForeignKeyModalForm.Designer.cs
└── ...
```

---

## 🔧 Checklist

### Designer
- [ ] Form modal (ShowInTaskbar = false, StartPosition = CenterParent)
- [ ] Label "Tabela de Referência:"
- [ ] ComboBox (Name: `comboBoxReferencedTable`)
- [ ] Label "Mapeamento de Colunas:"
- [ ] DataGridView (Name: `dataGridViewMapping`)
  - [ ] 2 colunas ComboBox (Campo Local, Campo Referência)
- [ ] Button "Adicionar Par" (Name: `buttonAddPair`)
- [ ] Button "Remover Par" (Name: `buttonRemovePair`)
- [ ] Button "Salvar FK" (Name: `buttonSaveFK`)
- [ ] Button "Cancelar" (Name: `buttonCancel`)

### Code-Behind
- [ ] Constructor recebe `Table currentTable`, `Project project`, `ForeignKey existingFK` (opcional)
- [ ] Método `InitializeComboBoxes()` — popula ComboBox de tabelas
- [ ] Método `InitializeDataGridView()` — configura DataGridView e preenche pares existentes
- [ ] Método `GetReferencedTable()` — retorna Table selecionada
- [ ] Evento `ComboBoxReferencedTable_SelectedIndexChanged` — atualiza ComboBox de campos
- [ ] Evento "Adicionar Par"
- [ ] Evento "Remover Par"
- [ ] Evento "Salvar FK" — valida e constrói ForeignKey
- [ ] Método público `GetForeignKey()` — retorna FK construída
- [ ] Evento "Cancelar"

---

## ✅ Testes

### Teste 1: Abrir Modal
```
1. TableEditorForm > Aba 3
2. Clique "Nova FK"
3. Modal abre
```

### Teste 2: Seleção de Tabela
```
1. ComboBox mostra todas as tabelas
2. Seleção muda ComboBox de campos referenciados
```

### Teste 3: Validação de Tipo
```
1. Campo local: INTEGER
2. Campo referência: STRING
3. Clique "Salvar"
4. Erro: "Tipos incompatíveis"
```

### Teste 4: FK Composta
```
1. Adicionar 2 pares
2. Salvar
3. FK contém 2 FieldsForeignKey
```

### Teste 5: Auto-relacionamento
```
1. Tabela "Users" referencia "Users"
2. Campo: ParentUserId → Users.Id
3. Salvar com sucesso
```

---

## 🔗 Próximos Passos

Após concluir Fase 7:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 8: Validações & Cascata** (`08_phase_validations.md`)

**Tempo Estimado:** 1.5-2 horas

**Verificação Final:**
- [ ] Modal compila
- [ ] Abre de TableEditorForm
- [ ] ComboBox funciona
- [ ] DataGridView permite adicionar/remover pares
- [ ] Validação de tipos funciona
- [ ] FK composta funciona
- [ ] Dados salvos retornam para TableEditorForm
