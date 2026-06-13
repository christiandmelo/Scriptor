# Fase 5: Interface Gráfica - Tela Principal (MainForm)

## 📌 Objetivo

Implementar a tela principal (MainForm) com menu, contexto do projeto (cliente/projeto), DataGridView de tabelas e botões de ações.

**Dependências:** Fase 2 (Serviços)  
**Status:** ⏳ Não iniciado

---

## 🎨 Layout da Tela

```
┌─────────────────────────────────────────────────────────────────────┐
│ Scriptor - Gerador de Scripts SQL                           [_][□][X]│
├─────────────────────────────────────────────────────────────────────┤
│ File    Actions    Help                                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│ Nome do Cliente: [________________]    Nome do Projeto: [________]│
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                      Tabelas do Projeto                            │
│ ┌──────────────────────────────────────────────────────────────┐  │
│ │ Nome Tabela    │ Colunas │ Índices │ Relacionamentos │      │  │
│ ├──────────────────────────────────────────────────────────────┤  │
│ │ Users          │    3    │    1    │       1         │      │  │
│ │ Orders         │    5    │    2    │       2         │      │  │
│ │                │         │         │                 │      │  │
│ └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐               │
│ │ Nova Tabela  │ │ Editar       │ │ Excluir      │               │
│ └──────────────┘ └──────────────┘ └──────────────┘               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Implementação

### Componentes WinForms

1. **MenuStrip** — Menu superior (File, Actions, Help)
2. **TextBox** — Nome do Cliente (leitura/escrita)
3. **TextBox** — Nome do Projeto (leitura/escrita)
4. **DataGridView** — Lista de tabelas (somente leitura, com duplo clique)
5. **Buttons** — Nova Tabela, Editar, Excluir

### Menu Structure

```
File
  ├─ Novo Projeto
  ├─ Salvar Projeto
  ├─ Carregar Projeto
  └─ Sair

Actions
  ├─ Gerar Scripts SQL Server
  ├─ Gerar Scripts Oracle
  └─ (Separador)
  └─ Preferências (opcional)

Help
  └─ Sobre
```

---

## 📝 Eventos Principais

| Evento | Ação |
|--------|------|
| Menu "Novo Projeto" | Limpa campos, cria novo projeto vazio |
| Menu "Salvar Projeto" | SaveFileDialog, `ProjectService.SaveProject()` |
| Menu "Carregar Projeto" | OpenFileDialog, `ProjectService.LoadProject()`, atualiza UI |
| Menu "Sair" | `Application.Exit()` |
| Menu "Gerar Scripts SQL Server" | Chama `SqlServerGenerator`, exibe em diálogo |
| Menu "Gerar Scripts Oracle" | Chama `OracleGenerator`, exibe em diálogo |
| Botão "Nova Tabela" | Abre `TableEditorForm` em modo novo |
| Duplo clique linha DataGridView | Abre `TableEditorForm` em modo edição |
| Botão "Editar" | Abre `TableEditorForm` para tabela selecionada |
| Botão "Excluir" | MessageBox confirmação, remove tabela |
| TextBox Cliente/Projeto mudou | Atualiza `ProjectService.GetCurrentProject()` |

---

## 💻 Pseudocódigo

```csharp
public partial class MainForm : Form
{
    private ProjectService _projectService;
    private DataTable _dataTableSource;

    public MainForm()
    {
        InitializeComponent();
        _projectService = ProjectService.GetInstance();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        InitializeDataGridView();
        CreateNewProject();  // Projeto padrão
    }

    private void InitializeDataGridView()
    {
        // Configurar colunas
        dataGridViewTables.Columns.Add("Nome", "Nome Tabela");
        dataGridViewTables.Columns.Add("Colunas", "Qtd Colunas");
        dataGridViewTables.Columns.Add("Indices", "Qtd Índices");
        dataGridViewTables.Columns.Add("Relacionamentos", "Qtd FKs");
        dataGridViewTables.MultiSelect = false;
        dataGridViewTables.ReadOnly = true;
    }

    private void RefreshDataGridView()
    {
        // Limpar
        dataGridViewTables.Rows.Clear();

        // Preencher
        var project = _projectService.GetCurrentProject();
        textBoxClient.Text = project.NameClient;
        textBoxProject.Text = project.NameProject;

        foreach (var table in project.Tables)
        {
            dataGridViewTables.Rows.Add(
                table.Name,
                table.Columns.Count,
                table.Indexes.Count,
                table.Fks.Count
            );
        }
    }

    private void CreateNewProject()
    {
        _projectService.CreateNewProject("Novo Cliente", "Novo Projeto");
        RefreshDataGridView();
    }

    private void SaveProject()
    {
        var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _projectService.SaveProject(dialog.FileName);
                MessageBox.Show("Projeto salvo com sucesso!", "Sucesso");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro");
            }
        }
    }

    private void LoadProject()
    {
        var dialog = new OpenFileDialog { Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _projectService.LoadProject(dialog.FileName);
                RefreshDataGridView();
                MessageBox.Show("Projeto carregado com sucesso!", "Sucesso");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar: {ex.Message}", "Erro");
            }
        }
    }

    private void GenerateSqlServer()
    {
        var project = _projectService.GetCurrentProject();
        var generator = new SqlServerGenerator();
        var ddl = generator.GenerateScripts(project);

        ShowScriptDialog("SQL Server DDL", ddl);
    }

    private void GenerateOracle()
    {
        var project = _projectService.GetCurrentProject();
        var generator = new OracleGenerator();
        var ddl = generator.GenerateScripts(project);

        ShowScriptDialog("Oracle DDL", ddl);
    }

    private void ShowScriptDialog(string title, string script)
    {
        var form = new Form
        {
            Text = title,
            Width = 800,
            Height = 600,
            StartPosition = FormStartPosition.CenterParent
        };

        var textBox = new TextBox
        {
            Text = script,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = new Font("Courier New", 10)
        };

        form.Controls.Add(textBox);
        form.ShowDialog(this);
    }

    private void NewTable()
    {
        var form = new TableEditorForm(null);  // null = novo
        if (form.ShowDialog() == DialogResult.OK)
        {
            RefreshDataGridView();
        }
    }

    private void EditTable()
    {
        if (dataGridViewTables.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione uma tabela");
            return;
        }

        var index = dataGridViewTables.SelectedRows[0].Index;
        var table = _projectService.GetCurrentProject().Tables[index];

        var form = new TableEditorForm(table);
        if (form.ShowDialog() == DialogResult.OK)
        {
            RefreshDataGridView();
        }
    }

    private void DeleteTable()
    {
        if (dataGridViewTables.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecione uma tabela");
            return;
        }

        if (MessageBox.Show("Tem certeza?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.No)
            return;

        var index = dataGridViewTables.SelectedRows[0].Index;
        var table = _projectService.GetCurrentProject().Tables[index];

        _projectService.RemoveTable(table.Id);
        RefreshDataGridView();
    }

    private void DataGridView_DoubleClick(object sender, EventArgs e)
    {
        EditTable();
    }

    private void TextBoxClient_TextChanged(object sender, EventArgs e)
    {
        _projectService.GetCurrentProject().NameClient = textBoxClient.Text;
    }

    private void TextBoxProject_TextChanged(object sender, EventArgs e)
    {
        _projectService.GetCurrentProject().NameProject = textBoxProject.Text;
    }
}
```

---

## 📁 Estrutura de Arquivos

```
Scriptor/
├── Main.cs              (Renomear para MainForm.cs ou refatorar)
├── Main.Designer.cs
└── UI/ (nova pasta)
    └── MainForm.cs (ou deixar em Scriptor/ raiz)
```

---

## 🔧 Checklist

### Designer (Visual Studio Form Designer)
- [ ] Form com tamanho apropriado (800x600 mínimo)
- [ ] MenuStrip com menu File, Actions, Help
- [ ] TextBox para Cliente (Name: `textBoxClient`)
- [ ] TextBox para Projeto (Name: `textBoxProject`)
- [ ] DataGridView (Name: `dataGridViewTables`)
  - [ ] 4 colunas (Nome, Colunas, Índices, Relacionamentos)
  - [ ] Propriedade ReadOnly = true
  - [ ] MultiSelect = false
- [ ] Button "Nova Tabela" (Name: `buttonNewTable`)
- [ ] Button "Editar" (Name: `buttonEdit`)
- [ ] Button "Excluir" (Name: `buttonDelete`)

### Code-Behind
- [ ] Campo `_projectService` inicializado em constructor
- [ ] Método `InitializeDataGridView()` configura colunas
- [ ] Método `RefreshDataGridView()` popula com dados
- [ ] Método `CreateNewProject()`
- [ ] Método `SaveProject()` com SaveFileDialog
- [ ] Método `LoadProject()` com OpenFileDialog
- [ ] Método `GenerateSqlServer()`
- [ ] Método `GenerateOracle()`
- [ ] Método `ShowScriptDialog()` abre diálogo com script
- [ ] Método `NewTable()` abre TableEditorForm
- [ ] Método `EditTable()` abre TableEditorForm com seleção
- [ ] Método `DeleteTable()` remove com confirmação
- [ ] Evento `DoubleClick` do DataGridView chama EditTable
- [ ] Evento `TextChanged` dos TextBox atualiza ProjectService

### Menu Events
- [ ] File > Novo Projeto → `CreateNewProject()`
- [ ] File > Salvar Projeto → `SaveProject()`
- [ ] File > Carregar Projeto → `LoadProject()`
- [ ] File > Sair → `Application.Exit()`
- [ ] Actions > Gerar Scripts SQL Server → `GenerateSqlServer()`
- [ ] Actions > Gerar Scripts Oracle → `GenerateOracle()`

---

## ✅ Testes

### Teste 1: Abrir Application
```
Resultado esperado: MainForm aparece com projeto vazio
```

### Teste 2: Inserir Dados (Cliente/Projeto)
```
1. Digitar "Acme" em Nome do Cliente
2. Digitar "CRM" em Nome do Projeto
3. Verificar que ProjectService reflete mudanças
```

### Teste 3: Novo Projeto
```
1. Menu File > Novo Projeto
2. DataGridView deve ficar vazio
3. Campos de cliente/projeto resetados
```

### Teste 4: Salvar/Carregar
```
1. File > Salvar Projeto
2. Escolher local e salvar
3. File > Carregar Projeto
4. Selecionar arquivo salvo
5. Dados devem ser restaurados idênticos
```

---

## 🔗 Próximos Passos

Após concluir Fase 5:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 6: Editor de Tabelas** (`06_phase_tableeditor.md`)

**Tempo Estimado:** 2-3 horas

**Verificação Final:**
- [ ] Aplicação compila sem erros
- [ ] MainForm abre sem crashes
- [ ] Menu funciona
- [ ] DataGridView exibe dados corretamente
- [ ] Salvar/Carregar JSON funciona
