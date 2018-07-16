namespace SB_BPCompositeDataEntity
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Linq;
    using Microsoft.Dynamics.AX.Metadata.XppCompiler;

    /// <summary>
    /// Class that describes the errors of CompositeDataEntity creation are found
    /// </summary>

    [DataContract]
    public class InvalidCompositeDataEntityDiagnosticItem : CustomDiagnosticItem
    {
        private const string InvalidCompositeDataEntityKey = "CompositeDataEntity";
        public const string DiagnosticMoniker = "InvalidCompositeDataEntity";

        public InvalidCompositeDataEntityDiagnosticItem(string path, string elementType, TextPosition textPosition, string message)
            : base(path, elementType, textPosition, DiagnosticType.BestPractices, Severity.Warning, DiagnosticMoniker, Messages.InvalidCompositeDataEntity, message)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("invalidmessage");
            }
            this.message = message;
        }

        [DataMember]
        public string message { get; private set; }

        // Serialization support.
        public InvalidCompositeDataEntityDiagnosticItem(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Hydrate the diagnostic item from the given XML element.
        /// </summary>
        /// <param name="itemSpecificNode">The XML element containing the diagnostic.</param>
        protected override void ReadItemSpecificFields(XElement itemSpecificNode)
        {
            this.message = base.ReadCustomField(itemSpecificNode, InvalidCompositeDataEntityKey);
        }

        /// <summary>
        /// Write the state into the given XML element.
        /// </summary>
        /// <param name="itemSpecificNode">The element into which the state is persisted.</param>
        protected override void WriteItemSpecificFields(XElement itemSpecificNode)
        {
            this.WriteCustomField(itemSpecificNode, InvalidCompositeDataEntityKey, this.message);
        }
    }
}