using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SB_BPCompositeDataEntity
{
    /// <summary>
    /// This Class transfers result message of checking each element of Composite Data Entity
    /// </summary>
    public class ResultMessageDTO
    {
        protected string elementPath = "";
        protected string elementType = "";
        protected string resultMessage = "";

        public ResultMessageDTO(string _elementPath, string _elementType, string _resultMessage)
        {
            this.elementPath = _elementPath;
            this.elementType = _elementType;
            this.resultMessage = _resultMessage;
        }
        public string getResultMessage
        {
            get
            {
                return this.resultMessage;
            }
        }
        public string getElementPath
        {
            get
            {
                return this.elementPath;
            }
        }
        public string getElementType
        {
            get
            {
                return this.elementType;
            }
        }
       
    }
}
