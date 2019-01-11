using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Meridium.EPiServer.Migration.Support {
    class ImportedPageTypeDeleter {
        private readonly IMigrationLog _logger;
        private readonly IContentTypeRepository _repo;
        private readonly IPropertyDefinitionRepository _definitionRepo;
        private Queue<ContentType> _contentTypeQueue = new Queue<ContentType>();
        public ImportedPageTypeDeleter(IMigrationLog logger) {
            _logger = logger;
            _repo = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            _definitionRepo = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>();
        }

        public void DeleteContentTypes(bool logOnly = true) {
            var operation = logOnly ? "Would delete" : "Deleting";

            _contentTypeQueue = new Queue<ContentType>(_repo.List().Where(IsImportedPageType));

            if (_contentTypeQueue.Count == 0) {
                _logger.Log("Found no page types to delete");
            }

            while (_contentTypeQueue.Any()) {
                var ctontenType = _contentTypeQueue.Dequeue();
                _logger.Log("{2} page type: [{0}] {1}", ctontenType.ID, ctontenType.Name, operation);

                if (!logOnly) {
                    TryDeletePageType(ctontenType);
                }
            }
        }

        private static bool IsImportedPageType(ContentType contentType) {
            // We use the assumption that imported page types has no associated model
            // and does not start with the string "sys" like the root and waste basket pages
            return contentType.ModelType == null 
                && !contentType.Name.StartsWith("sys", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryDeletePageType(ContentType contentType) {
            try {
                foreach (var propertyDefinition in _definitionRepo.List(contentType.ID)) {
                    _logger.Log("  Deleting property definition [{0}] {1}", 
                        propertyDefinition.ID, propertyDefinition.Name);
                    _definitionRepo.Delete(propertyDefinition);
                }
                _repo.Delete(contentType);
                return true;
            }
            catch (Exception e) {
                _logger.Log("Failed to delete page type [{0}] {1}", contentType.ID, contentType.Name);
                _logger.Log(e.ToString());
                return false;
            }
        }       
    }
}