using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace Meridium.EPiServer.Migration.Support {
    class ImportedPageTypeDeleter {
        public ImportedPageTypeDeleter(IMigrationLog logger) {
            _logger = logger;
            _repo = ServiceLocator.Current.GetInstance<PageTypeRepository>();
            _definitionRepo = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>();
        }

        public void DeletePageTypes(bool logOnly = true) {
            var operation = logOnly ? "Would delete" : "Deleting";

            _pageTypeQueue = new Queue<PageType>(_repo.List().Where(IsImportedPageType));

            if (_pageTypeQueue.Count == 0) {
                _logger.Log("Found no page types to delete");
            }

            while (_pageTypeQueue.Any()) {
                var pt = _pageTypeQueue.Dequeue();
                _logger.Log("{2} page type: [{0}] {1}", pt.ID, pt.Name, operation);

                if (!logOnly) {
                    TryDeletePageType(pt);
                }
            }
        }

        private static bool IsImportedPageType(PageType pageType) {
            // We use the assumption that imported page types has no associated model
            // and does not start with the string "sys" like the root and waste basket pages
            return pageType.ModelType == null 
                && !pageType.Name.StartsWith("sys", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryDeletePageType(PageType pageType) {
            try {
                foreach (var propertyDefinition in _definitionRepo.List(pageType.ID)) {
                    _logger.Log("  Deleting property definition [{0}] {1}", 
                        propertyDefinition.ID, propertyDefinition.Name);
                    _definitionRepo.Delete(propertyDefinition);
                }
                _repo.Delete(pageType);
                return true;
            }
            catch (Exception e) {
                _logger.Log("Failed to delete page type [{0}] {1}", pageType.ID, pageType.Name);
                _logger.Log(e.ToString());
                return false;
            }
        }

        private readonly IMigrationLog _logger;
        private readonly PageTypeRepository _repo;
        private readonly IPropertyDefinitionRepository _definitionRepo;
        private Queue<PageType> _pageTypeQueue = new Queue<PageType>();
    }
}