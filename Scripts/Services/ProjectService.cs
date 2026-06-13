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

        // Métodos de projeto em memória
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
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Nome do cliente não pode estar vazio", nameof(clientName));
            
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Nome do projeto não pode estar vazio", nameof(projectName));

            _currentProject = new Project
            {
                NameClient = clientName,
                NameProject = projectName
            };
        }

        // Operações em Tabelas
        public void AddTable(Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

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
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

            var json = SerializeToJson(_currentProject);
            File.WriteAllText(filePath, json);
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
