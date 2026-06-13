# Fase 6: Interface Gráfica - Editor de Tabelas (TableEditorForm)

## 📌 Objetivo

Implementar o formulário de edição de tabelas com 3 abas (Colunas, Índices, FKs), permitindo gerenciamento completo da estrutura.

**Dependências:** Fase 5 (MainForm)  
**Status:** ⏳ Não iniciado

---

## 🎨 Layout com TabControl

```
┌──────────────────────────────────────────────────────────┐
│ Editor de Tabela: [Users________________]        [_][□][X]│
├──────────────────────────────────────────────────────────┤
│                                                          │
│ [Colunas] [Índices] [FKs]                               │
│
├──────────────────────────────────────────────────────────┤
│ ABA 1: COLUNAS                                           │
│ ┌────────────────────────────────────────────────────┐  │
│ │ Nome │ Tipo │ Tamanho │ Precisão │ PK │ Nullable │  │
│ ├────────────────────────────────────────────────────┤  │
│ │ Id   │ INT  │        │          │ ✓  │          │  │
│ │ Email│ STR  │   255  │          │    │          │  │
│ └────────────────────────────────────────────────────┘  │
│ [Adicionar] [Remover]                                    │
│
│ ABA 2: ÍNDICES                                           │
│ ┌──────────────────┐  ┌─────────────────────────────┐   │
│ │ Nome Índice      │  │ Colunas do Índice           │   │
│ ├──────────────────┤  ├─────────────────────────────┤   │
│ │ IDX_Email ✓      │  │ ☐ Id                        │   │
│ │ IDX_Created      │  │ ☑ Email                     │   │
│ └──────────────────┘  │ ☐ CreatedAt                 │   │
│ [Novo] [Remover]     └─────────────────────────────┘   │
│
│ ABA 3: CHAVES ESTRANGEIRAS                             │
│ ┌────────────────────────────────────────────────────┐  │
│ │ Tabela Ref │ Mapeamento                            │  │
│ ├────────────────────────────────────────────────────┤  │
│ │ Users      │ UserId → Users.Id                     │  │
│ │ Orders     │ OrderId → Orders.Id, UserId → Users  │  │
│ └────────────────────────────────────────────────────┘  │
│ [Nova FK] [Remover]                                      │
│
├──────────────────────────────────────────────────────────┤
│                         [Salvar] [Cancelar]              │
└──────────────────────────────────────────────────────────┘
```

---

## 🔧 Implementação - Aba 1: Colunas

### DataGridView Colunas

| Coluna | Tipo | Editável | Descrição |
|--------|------|----------|-----------|
| Nome | TextBox | Sim | Nome da coluna |
| Tipo | ComboBox | Sim | STRING, INTEGER, DECIMAL, DATE, BOOLEAN, TEXT, BLOB |
| Tamanho | TextBox | Sim | Numérico (opcional) |
| Precisão | TextBox | Sim | Numérico (opcional, para DECIMAL) |
| PK | CheckBox | Sim | É chave primária? |
| Nullable | CheckBox | Sim | Permite NULL? |

### Botões
- **Adicionar Coluna:** Insere nova linha vazia
- **Remover Coluna:** Remove selecionada (com cascata para índices/FKs)

---

## 🔧 Implementação - Aba 2: Índices

### Lado Esquerdo: Lista de Índices
- DataGridView com coluna "Nome Índice"
- Seleção simples (MultiSelect = false)
- Botões: "Novo Índice", "Remover Índice"

### Lado Direito: Checklist de Colunas
- CheckedListBox exibindo todas as colunas da tabela
- Ao selecionar índice à esquerda, CheckedListBox marca colunas do índice
- Usuário pode marcar/desmarcar para definir índice

### Fluxo
1. Clique em "Novo Índice" → insere linha vazia à esquerda
2. Digitar nome do índice
3. Índice automaticamente selecionado
4. Marcar colunas no CheckedListBox
5. Ao mudar seleção de índice → atualiza CheckedListBox

---

## 🔧 Implementação - Aba 3: Chaves Estrangeiras

### DataGridView de FKs
| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Tabela Referência | TextBox (RO) | Nome da tabela referenciada |
| Mapeamento | TextBox (RO) | Descrição dos pares (Ex: "Id → Users.Id") |

### Botões
- **Nova FK:** Abre `ForeignKeyModalForm`
- **Remover FK:** Remove FK selecionada

---

## 💻 Pseudocódigo Principal

```csharp
public partial class TableEditorForm : Form
{
    private Table _table;  // null = novo
    private ProjectService _projectService;

    public TableEditorForm(Table existingTable)
    {
        InitializeComponent();
        _table = existingTable ?? new Table();
        _projectService = ProjectService.GetInstance();
    }

    private void TableEditorForm_Load(object sender, EventArgs e)
    {
        textBoxTableName.Text = _table.Name;
        InitializeColumnGrid();
        InitializeIndexUI();
        InitializeForeignKeyGrid();
        RefreshUI();
    }

    // ========== ABA 1: COLUNAS ==========
    private void InitializeColumnGrid()
    {
        // Configurar colunas do DataGridView
        dataGridViewColumns.Columns.Add("Nome", "Nome");
        dataGridViewColumns.Columns.Add("Tipo", "Tipo");
        // ... adicionar restantes
        
        // ComboBox column para Tipo
        var typeColumn = new DataGridViewComboBoxColumn
        {
            DataSource = new[] { "STRING", "INTEGER", "DECIMAL", "DATE", "BOOLEAN", "TEXT", "BLOB" }
        };
        dataGridViewColumns.Columns["Tipo"] = typeColumn;
    }

    private void AddColumn_Click()
    {
        var newColumn = new Column();
        _table.Columns.Add(newColumn);
        RefreshColumnGrid();
    }

    private void RemoveColumn_Click()
    {
        if (dataGridViewColumns.SelectedRows.Count == 0)
            return;

        var index = dataGridViewColumns.SelectedRows[0].Index;
        var column = _table.Columns[index];

        // Cascata: verificar uso
        var indexesUsing = ValidationService.CheckColumnUsageInIndexes(_table, column);
        var fksUsing = ValidationService.CheckColumnUsageInForeignKeys(_table, column);

        if (indexesUsing.Count > 0 || fksUsing.Count > 0)
        {
            var msg = $"Coluna está em {indexesUsing.Count} índices e {fksUsing.Count} FKs.\nRemover?";
            if (MessageBox.Show(msg, "Cascata", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            // Cascata
            foreach (var idx in indexesUsing)
                idx.Columns.Remove(column);
            foreach (var fk in fksUsing)
                fk.FieldsForeignKey.RemoveAll(f => f.ColumnId == column.Id);
        }

        _table.Columns.RemoveAt(index);
        RefreshColumnGrid();
        RefreshIndexUI();  // Atualizar CheckedListBox
        RefreshForeignKeyGrid();
    }

    // ========== ABA 2: ÍNDICES ==========
    private void InitializeIndexUI()
    {
        // DataGridView à esquerda para índices
        // CheckedListBox à direita para colunas
    }

    private void AddIndex_Click()
    {
        var newIndex = new Index { Name = "IDX_" };
        _table.Indexes.Add(newIndex);
        RefreshIndexUI();
    }

    private void RemoveIndex_Click()
    {
        if (dataGridViewIndexes.SelectedRows.Count == 0)
            return;

        var index = dataGridViewIndexes.SelectedRows[0].Index;
        _table.Indexes.RemoveAt(index);
        RefreshIndexUI();
    }

    private void IndexGrid_SelectionChanged()
    {
        if (dataGridViewIndexes.SelectedRows.Count == 0)
            return;

        var index = dataGridViewIndexes.SelectedRows[0].Index;
        var selectedIndex = _table.Indexes[index];

        // Atualizar CheckedListBox
        checkedListBoxColumns.Items.Clear();
        foreach (var column in _table.Columns)
        {
            var itemIndex = checkedListBoxColumns.Items.Add(column.Name);
            if (selectedIndex.Columns.Any(c => c.Id == column.Id))
                checkedListBoxColumns.SetItemChecked(itemIndex, true);
        }
    }

    private void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (dataGridViewIndexes.SelectedRows.Count == 0)
            return;

        var indexIndex = dataGridViewIndexes.SelectedRows[0].Index;
        var selectedIndex = _table.Indexes[indexIndex];
        var columnIndex = e.Index;
        var column = _table.Columns[columnIndex];

        if (e.NewValue == CheckState.Checked)
            selectedIndex.Columns.Add(column);
        else
            selectedIndex.Columns.Remove(column);
    }

    // ========== ABA 3: FOREIGN KEYS ==========
    private void AddFK_Click()
    {
        var form = new ForeignKeyModalForm(_table, _projectService.GetCurrentProject());
        if (form.ShowDialog() == DialogResult.OK)
        {
            var newFK = form.GetForeignKey();
            _table.Fks.Add(newFK);
            RefreshForeignKeyGrid();
        }
    }

    private void RemoveFK_Click()
    {
        if (dataGridViewFKs.SelectedRows.Count == 0)
            return;

        var index = dataGridViewFKs.SelectedRows[0].Index;
        _table.Fks.RemoveAt(index);
        RefreshForeignKeyGrid();
    }

    private void RefreshForeignKeyGrid()
    {
        dataGridViewFKs.Rows.Clear();
        var project = _projectService.GetCurrentProject();

        foreach (var fk in _table.Fks)
        {
            var refTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
            var refTableName = refTable?.Name ?? "?";

            var mapping = "";
            foreach (var field in fk.FieldsForeignKey)
            {
                var localCol = _table.Columns.FirstOrDefault(c => c.Id == field.ColumnId);
                var refCol = refTable?.Columns.FirstOrDefault(c => c.Id == field.ReferencedColumnId);

                if (localCol != null && refCol != null)
                    mapping += $"{localCol.Name} → {refTableName}.{refCol.Name}, ";
            }

            mapping = mapping.TrimEnd(',', ' ');
            dataGridViewFKs.Rows.Add(refTableName, mapping);
        }
    }

    // ========== GERAL ==========
    private void SaveTable_Click()
    {
        // Validar
        if (string.IsNullOrWhiteSpace(textBoxTableName.Text))
        {
            MessageBox.Show("Nome da tabela é obrigatório");
            return;
        }

        if (!ValidationService.ValidateName(textBoxTableName.Text))
        {
            MessageBox.Show("Nome inválido");
            return;
        }

        // Atualizar
        _table.Name = textBoxTableName.Text;

        // Salvar
        if (_table.Id == Guid.Empty)
            _projectService.AddTable(_table);
        else
            _projectService.UpdateTable(_table);

        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelButton_Click()
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
├── TableEditorForm.cs
├── TableEditorForm.Designer.cs
└── ...
```

---

## 🔧 Checklist

### Designer
- [ ] Form com TabControl (3 abas: Colunas, Índices, FKs)
- [ ] TextBox "Nome da Tabela" no topo
- [ ] **Aba 1 (Colunas):**
  - [ ] DataGridView com 6 colunas (Nome, Tipo, Tamanho, Precisão, PK, Nullable)
  - [ ] ComboBox column para Tipo
  - [ ] CheckBox column para PK e Nullable
  - [ ] Button "Adicionar Coluna"
  - [ ] Button "Remover Coluna"
- [ ] **Aba 2 (Índices):**
  - [ ] DataGridView à esquerda (Lista de índices)
  - [ ] CheckedListBox à direita (Colunas disponíveis)
  - [ ] Button "Novo Índice"
  - [ ] Button "Remover Índice"
- [ ] **Aba 3 (FKs):**
  - [ ] DataGridView com 2 colunas (Tabela Referência, Mapeamento)
  - [ ] Button "Nova FK"
  - [ ] Button "Remover FK"
- [ ] Button "Salvar Tabela"
- [ ] Button "Cancelar"

### Code-Behind
- [ ] Constructor recebe `Table` (null = novo)
- [ ] Método `InitializeColumnGrid()`
- [ ] Método `RefreshColumnGrid()`
- [ ] Evento "Adicionar Coluna"
- [ ] Evento "Remover Coluna" (com cascata)
- [ ] Método `InitializeIndexUI()`
- [ ] Evento "Novo Índice"
- [ ] Evento "Remover Índice"
- [ ] Evento DataGridView selecionar índice → atualizar CheckedListBox
- [ ] Evento CheckedListBox mudar → atualizar índice
- [ ] Método `InitializeForeignKeyGrid()`
- [ ] Evento "Nova FK" → abre ForeignKeyModalForm
- [ ] Evento "Remover FK"
- [ ] Evento "Salvar Tabela" — valida, atualiza ProjectService, fecha
- [ ] Evento "Cancelar" — fecha sem salvar

---

## ✅ Testes

### Teste 1: Abrir Editor (Novo)
```
1. Clique "Nova Tabela" em MainForm
2. TableEditorForm abre
3. Campos vazios, abas visíveis
```

### Teste 2: Abrir Editor (Edição)
```
1. Duplo clique em tabela no MainForm
2. TableEditorForm abre com dados preenchidos
```

### Teste 3: Adicionar Coluna
```
1. Aba 1 (Colunas)
2. Clique "Adicionar Coluna"
3. Linha vazia aparece
4. Preencher dados
```

### Teste 4: Cascata de Exclusão
```
1. Coluna em um índice
2. Clique remover coluna
3. Alerta aparece
4. Confirmar
5. Coluna removida de índice
```

### Teste 5: Novo Índice
```
1. Aba 2
2. Clique "Novo Índice"
3. Digite nome
4. Marque colunas
5. Índice criado
```

---

## 🔗 Próximos Passos

Após concluir Fase 6:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 7: Modal FK** (`07_phase_fkmodal.md`)

**Tempo Estimado:** 2-3 horas

**Verificação Final:**
- [ ] TableEditorForm compila
- [ ] Abre de MainForm sem erros
- [ ] Colunas podem ser adicionadas/removidas
- [ ] Índices funcionam com CheckedListBox
- [ ] Dados salvos corretamente
