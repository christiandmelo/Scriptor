# Fase 8: Validações Avançadas e Lógica de Cascata

## 📌 Objetivo

Implementar regras de negócio avançadas: cascata de exclusão, validação de integridade referencial e limpeza de dados órfãos.

**Dependências:** Fase 6 (TableEditorForm), Fase 7 (ForeignKeyModalForm)  
**Status:** ⏳ Não iniciado

---

## 🎯 Funcionalidades a Implementar

### 1. Cascata de Exclusão (Fase 6)

Quando um usuário tenta remover uma coluna que está em uso:

**Fluxo:**
```
1. Usuário clica "Remover Coluna"
2. ValidationService.CheckColumnUsageInIndexes() retorna indices
3. ValidationService.CheckColumnUsageInForeignKeys() retorna FKs
4. Se há uso: mostrar MessageBox com alerta
5. Se usuário confirma:
   - Remover coluna de todos os índices
   - Remover coluna de todas as FKs
   - Remover coluna da tabela
6. Atualizar UI (atualizar abas 2 e 3)
```

**Implementação em TableEditorForm:**

```csharp
private void RemoveColumn_Click()
{
    if (dataGridViewColumns.SelectedRows.Count == 0)
    {
        MessageBox.Show("Selecione uma coluna");
        return;
    }

    var index = dataGridViewColumns.SelectedRows[0].Index;
    var column = _table.Columns[index];

    // 1. Verificar uso
    var indexesUsing = ValidationService.CheckColumnUsageInIndexes(_table, column);
    var fksUsing = ValidationService.CheckColumnUsageInForeignKeys(_table, column);

    // 2. Alerta
    if (indexesUsing.Count > 0 || fksUsing.Count > 0)
    {
        var msg = $"Esta coluna está sendo usada em:\n" +
                  $"• {indexesUsing.Count} índice(s)\n" +
                  $"• {fksUsing.Count} FK(s)\n\n" +
                  $"Deseja remover a coluna? (Será removida dos índices/FKs automaticamente)";

        if (MessageBox.Show(msg, "Cascata de Exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) 
            != DialogResult.Yes)
            return;
    }

    // 3. Cascata: remover de índices
    foreach (var idx in indexesUsing)
    {
        idx.Columns.RemoveAll(c => c.Id == column.Id);

        // Se índice ficou vazio, remover
        if (idx.Columns.Count == 0)
            _table.Indexes.Remove(idx);
    }

    // 4. Cascata: remover de FKs
    foreach (var fk in fksUsing)
    {
        fk.FieldsForeignKey.RemoveAll(f => f.ColumnId == column.Id);

        // Se FK ficou vazia, remover
        if (fk.FieldsForeignKey.Count == 0)
            _table.Fks.Remove(fk);
    }

    // 5. Remover coluna
    _table.Columns.RemoveAt(index);

    // 6. Atualizar UI
    RefreshColumnGrid();
    RefreshIndexUI();       // Atualizar CheckedListBox
    RefreshForeignKeyGrid();  // Remover FKs órfãs
}
```

---

### 2. Validação de Integridade Referencial

Implementar em `ValidationService` e chamar ao desserializar JSON:

```csharp
/// <summary>
/// Valida consistência de todas as FKs no projeto
/// Remove FKs órfãs (referência a tabela/coluna inexistente)
/// </summary>
public static void ValidateForeignKeyConsistency(Project project)
{
    foreach (var table in project.Tables)
    {
        var invalidFks = new List<ForeignKey>();

        foreach (var fk in table.Fks)
        {
            // 1. Verificar se tabela referenciada existe
            var referencedTable = project.Tables.FirstOrDefault(t => t.Id == fk.ReferencedTableId);
            if (referencedTable == null)
            {
                // FK órfã: tabela referenciada não existe
                invalidFks.Add(fk);
                continue;
            }

            // 2. Verificar se todas as colunas existem
            var invalidFields = new List<FieldsForeignKey>();
            foreach (var field in fk.FieldsForeignKey)
            {
                var localCol = table.Columns.FirstOrDefault(c => c.Id == field.ColumnId);
                var refCol = referencedTable.Columns.FirstOrDefault(c => c.Id == field.ReferencedColumnId);

                // Campo órfão: coluna local ou referenciada não existe
                if (localCol == null || refCol == null)
                    invalidFields.Add(field);
            }

            // 3. Remover campos órfãos
            foreach (var field in invalidFields)
            {
                fk.FieldsForeignKey.Remove(field);
                // Log: "Removido campo órfão em FK de {table.Name}"
            }

            // 4. Se FK ficou vazia, marcar para remoção
            if (fk.FieldsForeignKey.Count == 0)
                invalidFks.Add(fk);
        }

        // 5. Remover FKs órfãs
        foreach (var fk in invalidFks)
        {
            table.Fks.Remove(fk);
            // Log: "Removida FK órfã de {table.Name}"
        }
    }

    // 6. Validar índices (remover colunas órfãs)
    foreach (var table in project.Tables)
    {
        var invalidIndexes = new List<Index>();

        foreach (var index in table.Indexes)
        {
            var validColumns = index.Columns
                .Where(col => table.Columns.Any(c => c.Id == col.Id))
                .ToList();

            if (validColumns.Count != index.Columns.Count)
            {
                // Algumas colunas não existem mais
                index.Columns = validColumns;
            }

            // Se índice ficou sem colunas, remover
            if (index.Columns.Count == 0)
                invalidIndexes.Add(index);
        }

        foreach (var index in invalidIndexes)
        {
            table.Indexes.Remove(index);
            // Log: "Removido índice órfão {index.Name} de {table.Name}"
        }
    }
}
```

**Onde chamar:**
- `ProjectService.LoadProject()` — após desserializar JSON
- `ProjectService.CreateNewProject()` — (opcional, projetos novos não têm problemas)

---

### 3. Validação de FK Composta

Validar em `ForeignKeyModalForm` antes de salvar:

```csharp
private void SaveFK_Click(object sender, EventArgs e)
{
    // ... (validações existentes)

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
            MessageBox.Show($"Coluna inválida");
            return;
        }

        // ✅ VALIDAÇÃO: Tipos compatíveis
        if (!ValidationService.ValidateForeignKeyMapping(localCol, refCol))
        {
            MessageBox.Show(
                $"Erro de tipo em '{localColName}':\n" +
                $"Local: {localCol.DataType}({localCol.Size})\n" +
                $"Referência: {refCol.DataType}({refCol.Size})\n\n" +
                $"Os tipos devem ser idênticos (incluindo tamanho)",
                "Validação",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        fieldsList.Add(new FieldsForeignKey
        {
            ColumnId = localCol.Id,
            ReferencedColumnId = refCol.Id
        });
    }

    // ✅ VALIDAÇÃO: Sem pares duplicados
    var uniquePairs = new HashSet<string>();
    foreach (var field in fieldsList)
    {
        var pairKey = $"{field.ColumnId}|{field.ReferencedColumnId}";
        if (uniquePairs.Contains(pairKey))
        {
            MessageBox.Show("Não é permitido mapear a mesma coluna duas vezes");
            return;
        }
        uniquePairs.Add(pairKey);
    }

    _foreignKey.ReferencedTableId = refTable.Id;
    _foreignKey.FieldsForeignKey = fieldsList;

    DialogResult = DialogResult.OK;
    Close();
}
```

---

### 4. Integração em ProjectService

```csharp
public void LoadProject(string filePath)
{
    _currentProject = JsonSerializationService.DeserializeFromFile(filePath);
    
    // ✅ Validar integridade após carregar
    ValidationService.ValidateForeignKeyConsistency(_currentProject);
}
```

---

## 🔧 Checklist de Implementação

### ValidationService Estendido
- [ ] Método `CheckColumnUsageInIndexes(table, column)` — já existe, verificar se funciona
- [ ] Método `CheckColumnUsageInForeignKeys(table, column)` — já existe, verificar se funciona
- [ ] Método `ValidateForeignKeyConsistency(project)` — NOVO
- [ ] Método `ValidateForeignKeyMapping(col1, col2)` — já existe, verificar se funciona

### TableEditorForm Modificado
- [ ] Evento "Remover Coluna" implementado com cascata
- [ ] Alerta exibido (MessageBox)
- [ ] Indices atualizadas após cascata
- [ ] ForeignKeys atualizadas após cascata
- [ ] UI atualizada (todas as 3 abas)

### ForeignKeyModalForm Modificado
- [ ] Validação de tipos compatíveis em "Salvar FK"
- [ ] Validação de pares duplicados
- [ ] Mensagens de erro clara e específicas

### ProjectService Modificado
- [ ] Método `LoadProject()` chama `ValidationService.ValidateForeignKeyConsistency()`

### Logging (Opcional)
- [ ] Se houver cascata, exibir alerta detalhado
- [ ] Se houver FKs órfãs ao carregar, exibir aviso

---

## ✅ Testes

### Teste 1: Cascata Simples
```
1. Criar tabela com coluna "Email" em índice
2. Tentar remover "Email"
3. Alerta exibido: "Coluna está em 1 índice"
4. Confirmar
5. Coluna removida, índice atualizado
```

### Teste 2: Cascata Múltipla
```
1. Coluna "Id" em 2 índices + 1 FK
2. Remover "Id"
3. Alerta: "Coluna está em 2 índices e 1 FK"
4. Confirmar
5. Tudo removido em cascata
```

### Teste 3: Cancelar Cascata
```
1. Coluna em índice
2. Remover coluna
3. Alerta
4. Clique "Não"
5. Coluna NÃO é removida
```

### Teste 4: Validação de Tipo (FK)
```
1. Campo local: STRING(255)
2. Campo ref: INTEGER
3. Clique "Salvar FK"
4. Erro: "Tipos incompatíveis"
```

### Teste 5: FK Órfã Removida ao Carregar
```
1. Salvar projeto com FK válida
2. Editar arquivo JSON: remover coluna referenciada
3. Carregar projeto
4. FK é removida automaticamente (integridade validada)
```

### Teste 6: Índice Órfão Removido
```
1. Índice com 2 colunas
2. Desserializar JSON onde 1 coluna não existe
3. Índice é mantido com 1 coluna válida
4. Se índice ficar vazio, é removido
```

---

## 📝 Casos Extremos

| Caso | Ação | Resultado |
|------|------|-----------|
| Remover PK que está em índice | Cascata | Índice atualizado |
| Remover coluna única de FK | Cascata | FK removida |
| Remover tabela referenciada por FK | (não permitido em UI) | Validação ao carregar |
| Auto-relacionamento, remover coluna | Cascata | FK removida |
| Índice sem colunas | Auto-remove | Não aparece em UI |
| FK sem campos | Auto-remove | Não aparece em UI |

---

## 🔗 Próximos Passos

Após concluir Fase 8:
1. ✅ Marcar como concluída em `PLANNING.md`
2. ➡️ Ir para **Fase 9: Testes & Integração** (`09_phase_testing.md`)

**Tempo Estimado:** 1-1.5 horas

**Verificação Final:**
- [ ] Compilar sem erros
- [ ] 6 testes acima executados com sucesso
- [ ] Cascata funciona corretamente
- [ ] Validação de integridade funciona ao carregar
- [ ] FKs órfãs são removidas automaticamente
