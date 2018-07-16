using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;


namespace SB_BPCompositeDataEntity
{
    /// <summary>
    /// This class checks Composite Data Entity and presents the results
    /// </summary>
    public class CompositeDataEntityCheckerAx
    {
        /// <summary>
        ///     Global variables representing the current DataEntity, result list and string parameters for Best Practice rule message
        /// </summary>
        protected AxCompositeDataEntityView compositeDataEntityToCheck;
        protected List<ResultMessageDTO> resutlMessage = new List<ResultMessageDTO>();
        protected string rootDataEntityPath;
        protected string rootDataEntityType;
        public CompositeDataEntityCheckerAx(string inputDataEntityName)
        {
            if (inputDataEntityName != null)
            {
                compositeDataEntityToCheck = DesignMetaModelService.Instance.GetCompositeDataEntityView(inputDataEntityName);
                rootDataEntityPath = $"{compositeDataEntityToCheck.GetType().ToString().Split('.').Last()}/{compositeDataEntityToCheck.Name}";
                rootDataEntityType = $"{compositeDataEntityToCheck.GetType().ToString().Split('.').Last()}";
            }
            
        }
        public List<ResultMessageDTO> getResultMessageDTO
        {
            get { return resutlMessage; }
        }
        /// <summary>
        /// This method finds root Data Entity. Check it and call check for embedded DataEntity
        /// </summary>
        public void checkDataEntity()
        {
            foreach(AxDataEntityViewReference rootReference in compositeDataEntityToCheck.RootDataEntities)
            {
                this.isDataManagmentAvailable(rootReference);
                this.checkStagingTableForDataEntity(rootReference);
                this.checkEmbeddedDataEntities(rootReference);
            }
        }
        /// <summary>
        /// This method check embedded Data Entity for root Data Entity in recusion
        /// </summary>
        /// <param name="rootReference"> This is reference to parent Data Entity</param>
        protected void checkEmbeddedDataEntities(AxDataEntityViewReference rootReference)
        {
            foreach (AxDataEntityViewReference embeddedElements in rootReference.EmbeddedDataEntities)
            {
                if (this.isDataManagmentAvailable(embeddedElements))
                {
                    if (!this.checkDataEntityRelations(embeddedElements, rootReference.DataEntity))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{embeddedElements.DataEntity}",rootDataEntityType, $"Invalid relations between Data Entity - {embeddedElements.DataEntity} and Data Entity - {rootReference.DataEntity}"));

                    }
                    this.checkStagingTableForDataEntity(embeddedElements);
                    if (!this.checkStagingTableRelations(embeddedElements, this.getStagingTable(rootReference.DataEntity), rootReference.DataEntity))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{embeddedElements.DataEntity}/{this.getStagingTable(embeddedElements.DataEntity)}",rootDataEntityType, $"Invalid relations between table - {this.getStagingTable(embeddedElements.DataEntity)} and table - {this.getStagingTable(rootReference.DataEntity)}"));
                    }
                    if (!this.checkEntityDataSourceRelations(embeddedElements, rootReference))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{embeddedElements.DataEntity}/Data Sources",rootDataEntityType, $"Invalid relations between DataSources of Data Entity - {embeddedElements.DataEntity} and Data Entity - {rootReference.DataEntity}"));
                    }
                }
                this.checkEmbeddedDataEntities(embeddedElements);
            }
        }
        /// <summary>
        /// This method checks if Data Sources (mostly, main table for Data Entity) have relation
        /// </summary>
        /// <param name="embeddedElements">reference to child Data Entity</param>
        /// <param name="rootReference">reference to parent Data Entity</param>
        /// <returns>True if relation is found</returns>
        protected bool checkEntityDataSourceRelations(AxDataEntityViewReference embeddedElements, AxDataEntityViewReference rootReference)
        {
            bool ret = false;
            List<string> embeddedEntityDataSource = new List<string>();
            List<string> parentEntityDataSource = new List<string>();
            AxTable relatedTable;
            AxDataEntityView dataEntity;
            embeddedEntityDataSource = this.getEntityDataSource(embeddedElements);
            parentEntityDataSource = this.getEntityDataSource(rootReference);

            foreach (string enbeddedTable in embeddedEntityDataSource)
            {
                relatedTable = DesignMetaModelService.Instance.GetTable(enbeddedTable);
                foreach (AxTableRelation relation in relatedTable.Relations)
                {
                    if (parentEntityDataSource.Exists(x => x.Contains(relation.RelatedTable)))
                    {
                        ret = true;
                    }

                }
            }
            if (!ret)
            {
                dataEntity = DesignMetaModelService.Instance.GetDataEntityView(embeddedElements.DataEntity);
                foreach (AxQuerySimpleDataSource query in dataEntity.ViewMetadata.DataSources)
                {
                    embeddedEntityDataSource = this.getEntityDataSourceRecursion(query, embeddedEntityDataSource);
                }

                foreach (string dataSource in embeddedEntityDataSource)
                {
                    if (parentEntityDataSource.Exists(x => x.Contains(dataSource)))
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// This method finds all Data Sources (embedded) for Data Entity in recursion
        /// </summary>
        /// <param name="dataSource">AX object DataSource on Data Entity</param>
        /// <param name="listDataSources">list to fill</param>
        /// <returns>list of all Data Sources for Data Entity</returns>
        protected List<string> getEntityDataSourceRecursion(AxQuerySimpleDataSource dataSource, List<string> listDataSources)
        {
            foreach (AxQuerySimpleDataSource query in dataSource.DataSources)
            {
                listDataSources.Add(query.Table);
                this.getEntityDataSourceRecursion(query, listDataSources);
            }
            return listDataSources;
        }
        /// <summary>
        /// This method finds DataSource (table on whitch Data Entity was built)
        /// </summary>
        /// <param name="reference">reference to Data Entity</param>
        /// <returns>list of DataSources for Data Entity</returns>
        protected List<string> getEntityDataSource(AxDataEntityViewReference reference)
        {
            List<string> dataSourceList = new List<string>();
            AxDataEntityView dataEntity = DesignMetaModelService.Instance.GetDataEntityView(reference.DataEntity);

            foreach (AxQuerySimpleDataSource query in dataEntity.ViewMetadata.DataSources)
            {
                dataSourceList.Add(query.Table);
            }

            return dataSourceList;
        }
        /// <summary>
        /// This method chekcs if there are relations between Staging tables child Data Entity and parent Data Entity
        /// </summary>
        /// <param name="embeddedElements">reference to child DataEntity</param>
        /// <param name="rootStagingTable">Name of parent Data Entity staging table</param>
        /// <param name="rootDataEntity">Name of root Data Entity</param>
        /// <returns>True if Staging tables have relations</returns>
        protected bool checkStagingTableRelations(AxDataEntityViewReference embeddedElements, string rootStagingTable, string rootDataEntity)
        {
            bool ret = false;
            AxTable embeddedStagingTable = DesignMetaModelService.Instance.GetTable(this.getStagingTable(embeddedElements.DataEntity));
            AxTable parentStagingTable = DesignMetaModelService.Instance.GetTable(rootStagingTable);
            Dictionary<string, string> relatedFields = new Dictionary<string, string>();

            foreach (AxTableRelation relateTable in embeddedStagingTable.Relations)
            {
                if (relateTable.RelatedTable == rootStagingTable)
                {
                    ret = true;
                    foreach (AxTableRelationConstraintField field in relateTable.Constraints)
                    {
                        relatedFields.Add(field.Field, field.RelatedField);
                    }
                }
            }
            if (ret && (relatedFields.Count() > 0))
            {
                ret = false;
                foreach (AxTableRelation relateTable in parentStagingTable.Relations)
                {
                    if (relateTable.RelatedTable == rootDataEntity)
                    {
                        foreach (AxTableRelationConstraintField field in relateTable.Constraints)
                        {
                            if (!field.Field.Equals("DefinitionGroup", System.StringComparison.OrdinalIgnoreCase) && !field.Field.Equals("ExecutionId", System.StringComparison.OrdinalIgnoreCase) && relatedFields.ContainsValue(field.Field))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// This method checks if parent Data Entity and child Data Entity has the relation
        /// </summary>
        /// <param name="embeddedElements"> This is reference to child Data Entity</param>
        /// <param name="parentDataEntityName">This is name of parent Data Entity</param>
        /// <returns>If the relation exists method reterns true</returns>
        protected bool checkDataEntityRelations(AxDataEntityViewReference embeddedElements, string parentDataEntityName)
        {
            AxDataEntityView dataEntityViewName;
            AxDataEntityViewExtension dataEntityViewExtName;
            bool ret = false;
            dataEntityViewName = DesignMetaModelService.Instance.GetDataEntityView(embeddedElements.DataEntity);
            if (dataEntityViewName != null)
            {

                foreach (AxDataEntityViewRelation relation in dataEntityViewName.Relations)
                {
                    if (relation.RelatedDataEntity == parentDataEntityName)
                    {
                        ret = true;
                    }
                }
            }
            dataEntityViewExtName = DesignMetaModelService.Instance.GetDataEntityViewExtension(embeddedElements.DataEntity);
            if (dataEntityViewExtName != null)
            {
                foreach (AxDataEntityViewRelation relation in dataEntityViewExtName.Relations)
                {
                    if (relation.RelatedDataEntity == parentDataEntityName)
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// This method checks if the Staging table related to Data Entity and has all necessary fields and indexes
        /// </summary>
        /// <param name="rootReference">This is reference to Data Entity</param>
        protected void checkStagingTableForDataEntity(AxDataEntityViewReference rootReference)
        {
            AxTable stagingTable = DesignMetaModelService.Instance.GetTable(this.getStagingTable(rootReference.DataEntity));
            List<string> relatedFields = new List<string>();
            foreach (AxTableRelation relation in stagingTable.Relations)
            {
                if (relation.RelatedTable == rootReference.DataEntity && relation.Cardinality == Cardinality.ExactlyOne && relation.RelatedTableCardinality == RelatedTableCardinality.ZeroOne)
                {
                    foreach (AxTableRelationConstraintField field in relation.Constraints)
                    {
                        relatedFields.Add(field.Field);
                    }
                    if (!this.checkStagingTableUniqueIndex(stagingTable, relatedFields))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{rootReference.DataEntity}/{stagingTable.Name}/Index", rootDataEntityType, $"Invalid Index on {stagingTable.Name}"));
                    }
                    if (!this.checkMandatoryFieldsStagingTable(stagingTable))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{rootReference.DataEntity}/{stagingTable.Name}/Fields", rootDataEntityType, $"Not all necessary fields are in table - {stagingTable.Name}"));
                    }
                    if (!this.checkStagingTableRowIdIndex(stagingTable))
                    {
                        resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{rootReference.DataEntity}/{stagingTable.Name}/Index", rootDataEntityType, $"Invalid RowId Index on {stagingTable.Name}"));
                    }
                }
            }
        }
        /// <summary>
        /// This method checks if Staging table has RowId index. It is recommended for performance.
        /// </summary>
        /// <param name="stagingTable">AOT Staging table</param>
        /// <returns>True if index exists</returns>
        protected bool checkStagingTableRowIdIndex(AxTable stagingTable)
        {
            bool ret = false;
            bool executionId = false;
            bool definitionGroup = false;
            bool parentRowId = false;
            bool rowId = false;
            List<string> indexes = new List<string>();
            foreach (AxTableIndex index in stagingTable.Indexes)
            {
                foreach (AxTableIndexField indexField in index.Fields)
                {
                    if (indexField.DataField.Equals("ExecutionId", System.StringComparison.OrdinalIgnoreCase))
                    {
                        executionId = true;
                    }
                    if (indexField.DataField.Equals("DefinitionGroup", System.StringComparison.OrdinalIgnoreCase))
                    {
                        definitionGroup = true;
                    }
                    if (indexField.Name.Equals("ParentRowId", System.StringComparison.OrdinalIgnoreCase))
                    {
                        parentRowId = true;
                    }
                      if(indexField.Name.Equals("RowId", System.StringComparison.OrdinalIgnoreCase))
                    {
                        rowId = true;
                    }
                  if (executionId && definitionGroup && parentRowId && rowId)
                    {
                        ret = true;
                        return ret;
                    }
                }
                executionId = false;
                definitionGroup = false;
                parentRowId = false;
                rowId = false;
            }
            return ret;
        }
        /// <summary>
        /// This method checks if Staging table has 4 fields witch are needed for Composite Data Entity 
        /// </summary>
        /// <param name="stagingTable">AOT Staging table</param>
        /// <returns>True if all 4 fields are in the table</returns>
        protected bool checkMandatoryFieldsStagingTable(AxTable stagingTable)
        {
            bool ret = false;
            List<string> fieldsList = new List<string>();
            foreach (AxTableField field in stagingTable.Fields)
            {
                if (field.Name.Equals("ParentRowId", System.StringComparison.OrdinalIgnoreCase) || field.Name.Equals("RowId", System.StringComparison.OrdinalIgnoreCase) || field.Name.Equals("ExecutionId", System.StringComparison.OrdinalIgnoreCase) || field.Name.Equals("DefinitionGroup", System.StringComparison.OrdinalIgnoreCase))
                {
                    fieldsList.Add(field.Name.ToLower());
                }
            }

            if (fieldsList.Exists(x => x.Contains("parentrowid")) && fieldsList.Exists(x => x.Contains("rowid")) && fieldsList.Exists(x => x.Contains("executionid")) && fieldsList.Exists(x => x.Contains("definitiongroup")))
            {
                ret = true;
            }

            return ret;
        }
        /// <summary>
        /// This method checks if Staging table has the specific index witch includes field that provides relation between Staging table and Data Entity 
        /// </summary>
        /// <param name="stagingTable">AOT staging table</param>
        /// <param name="relatedFields">related fields list</param>
        /// <returns>True if index exists</returns>
        protected bool checkStagingTableUniqueIndex(AxTable stagingTable, List<string> relatedFields)
        {
            bool ret = false;
            foreach (AxTableIndex stagingIndex in stagingTable.Indexes)
            {
                if ((stagingIndex.AllowDuplicates == NoYes.No && stagingIndex.AlternateKey == NoYes.Yes) || stagingIndex.Name.Equals("StagingIdx", System.StringComparison.OrdinalIgnoreCase))
                {
                    foreach (AxTableIndexField tableField in stagingIndex.Fields)
                    {
                        if ((!tableField.DataField.Equals("DefinitionGroup", System.StringComparison.OrdinalIgnoreCase) || !tableField.DataField.Equals("ExecutionId", System.StringComparison.OrdinalIgnoreCase)) && relatedFields.Exists(x => x.Contains(tableField.DataField)))
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// This method finds Staging table name in AOT for Data Entity
        /// </summary>
        /// <param name="dataEntity">This is AOT name of Data Entity</param>
        /// <returns>Staging table name in AOT for Data Entity</returns>
        protected string getStagingTable(string dataEntity)
        {
            AxDataEntityView dataEntityViewName;
            string ret = "";

            dataEntityViewName = DesignMetaModelService.Instance.GetDataEntityView(dataEntity);
            if (dataEntityViewName != null)
            {
                ret = dataEntityViewName.DataManagementStagingTable;
            }

            return ret;
        }
        /// <summary>
        /// This method checks if proterty "Data management enabled" is true, 
        /// it shows that this Data Entity has Staging table and can be use in Composite Data Entity  
        /// </summary>
        /// <param name="rootReference">This is reference to Data Entity</param>
        /// <returns>True if proterty "Data management enabled" is enable</returns>
        protected bool isDataManagmentAvailable(AxDataEntityViewReference rootReference)
        {
            AxDataEntityView dataEntity;
            bool ret = false;
            dataEntity = DesignMetaModelService.Instance.GetDataEntityView(rootReference.DataEntity);
            if (dataEntity != null)
            {
                if (dataEntity.DataManagementEnabled == NoYes.Yes)
                {
                    ret = true;
                }
                if (!ret)
                {
                    resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{rootReference.DataEntity}", rootDataEntityType, $"Property Data Managment is disabled. Enable it for {dataEntity.Name}"));
                }
            }
            else
            {
                resutlMessage.Add(new ResultMessageDTO($"{rootDataEntityPath}/{rootReference.DataEntity}", rootDataEntityType, $"Data Entity {rootReference.DataEntity} does not exist"));
            }

            return ret;
        }
    }
}
