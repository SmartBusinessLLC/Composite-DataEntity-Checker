namespace SB_BPCompositeDataEntity
{
    using System.Collections.Generic;
    using Microsoft.Dynamics.AX.Framework.BestPractices.Extensions;
    using Microsoft.Dynamics.AX.Metadata.MetaModel;


    [BestPracticeRule(
       InvalidCompositeDataEntityDiagnosticItem.DiagnosticMoniker,
       typeof(Messages),
       InvalidCompositeDataEntityDiagnosticItem.DiagnosticMoniker + "Description",
       BestPracticeCheckerTargets.CompositeDataEntityView)]
    public class CompositeDataEntityMetadataCheck : BestPracticeMetadataElementChecker<AxCompositeDataEntityView>
    {
        /// <summary>
        /// This method is called with the top level artifacts that need
        /// to be checked. In this implementation, we are only interested
        /// in CompositeDataEntities - everything else is ignored. 
        /// </summary>
        /// <param name="metaObject">A metadata instance to check for BP violations.</param>
        public override void RunChecksOn(Microsoft.Dynamics.AX.Metadata.Core.MetaModel.INamedObject metaObject)
        {
            AxCompositeDataEntityView dataEntityToCheck = metaObject as AxCompositeDataEntityView;
            CompositeDataEntityCheckerAx checker = new CompositeDataEntityCheckerAx(dataEntityToCheck.Name);
            checker.checkDataEntity();
            this.VisitCompositeDataEntity(checker.getResultMessageDTO);
        }

        protected override void RunChecksOn(AxCompositeDataEntityView element)
        {
        }

        /// <summary>
        /// Implementation of the CompositeDataEntity checker
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="field"></param>
        private void VisitCompositeDataEntity(List<ResultMessageDTO> results)
        {
            if (results.Count<1)
            {
                // nothing to check.
                return;
            }
            foreach (ResultMessageDTO dto in results)
            {
                // Build a diagnostic ...
                InvalidCompositeDataEntityDiagnosticItem diagnostic = new InvalidCompositeDataEntityDiagnosticItem(
                    dto.getElementPath,
                    dto.getElementType,
                    null,
                    dto.getResultMessage);
                // ... and report the error.
                this.ExtensionContext.AddErrorMessage(diagnostic);

            }
        }
    }
}