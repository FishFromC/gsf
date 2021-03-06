//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1Choice(Name = "ControlElement")]
    public class ControlElement : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ControlElement));
        private BeginDomainDefSequenceType beginDomainDef_;
        private bool beginDomainDef_selected;


        private ContinueDomainDefSequenceType continueDomainDef_;
        private bool continueDomainDef_selected;


        private Identifier endDomainDef_;
        private bool endDomainDef_selected;


        private PiDefinitionSequenceType piDefinition_;
        private bool piDefinition_selected;

        [ASN1Element(Name = "beginDomainDef", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public BeginDomainDefSequenceType BeginDomainDef
        {
            get
            {
                return beginDomainDef_;
            }
            set
            {
                selectBeginDomainDef(value);
            }
        }

        [ASN1Element(Name = "continueDomainDef", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public ContinueDomainDefSequenceType ContinueDomainDef
        {
            get
            {
                return continueDomainDef_;
            }
            set
            {
                selectContinueDomainDef(value);
            }
        }

        [ASN1Element(Name = "endDomainDef", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public Identifier EndDomainDef
        {
            get
            {
                return endDomainDef_;
            }
            set
            {
                selectEndDomainDef(value);
            }
        }


        [ASN1Element(Name = "piDefinition", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
        public PiDefinitionSequenceType PiDefinition
        {
            get
            {
                return piDefinition_;
            }
            set
            {
                selectPiDefinition(value);
            }
        }

        public void initWithDefaults()
        {
        }

        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }


        public bool isBeginDomainDefSelected()
        {
            return beginDomainDef_selected;
        }


        public void selectBeginDomainDef(BeginDomainDefSequenceType val)
        {
            beginDomainDef_ = val;
            beginDomainDef_selected = true;


            continueDomainDef_selected = false;

            endDomainDef_selected = false;

            piDefinition_selected = false;
        }


        public bool isContinueDomainDefSelected()
        {
            return continueDomainDef_selected;
        }


        public void selectContinueDomainDef(ContinueDomainDefSequenceType val)
        {
            continueDomainDef_ = val;
            continueDomainDef_selected = true;


            beginDomainDef_selected = false;

            endDomainDef_selected = false;

            piDefinition_selected = false;
        }


        public bool isEndDomainDefSelected()
        {
            return endDomainDef_selected;
        }


        public void selectEndDomainDef(Identifier val)
        {
            endDomainDef_ = val;
            endDomainDef_selected = true;


            beginDomainDef_selected = false;

            continueDomainDef_selected = false;

            piDefinition_selected = false;
        }


        public bool isPiDefinitionSelected()
        {
            return piDefinition_selected;
        }


        public void selectPiDefinition(PiDefinitionSequenceType val)
        {
            piDefinition_ = val;
            piDefinition_selected = true;


            beginDomainDef_selected = false;

            continueDomainDef_selected = false;

            endDomainDef_selected = false;
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "beginDomainDef", IsSet = false)]
        public class BeginDomainDefSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(BeginDomainDefSequenceType));
            private ICollection<MMSString> capabilities_;
            private Identifier domainName_;
            private LoadData loadData_;

            private bool loadData_present;
            private bool sharable_;

            [ASN1Element(Name = "domainName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public Identifier DomainName
            {
                get
                {
                    return domainName_;
                }
                set
                {
                    domainName_ = value;
                }
            }


            [ASN1SequenceOf(Name = "capabilities", IsSetOf = false)]
            [ASN1Element(Name = "capabilities", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
            public ICollection<MMSString> Capabilities
            {
                get
                {
                    return capabilities_;
                }
                set
                {
                    capabilities_ = value;
                }
            }


            [ASN1Boolean(Name = "")]
            [ASN1Element(Name = "sharable", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public bool Sharable
            {
                get
                {
                    return sharable_;
                }
                set
                {
                    sharable_ = value;
                }
            }


            [ASN1Element(Name = "loadData", IsOptional = true, HasTag = true, Tag = 4, HasDefaultValue = false)]
            public LoadData LoadData
            {
                get
                {
                    return loadData_;
                }
                set
                {
                    loadData_ = value;
                    loadData_present = true;
                }
            }


            public void initWithDefaults()
            {
            }

            public IASN1PreparedElementData PreparedData
            {
                get
                {
                    return preparedData;
                }
            }

            public bool isLoadDataPresent()
            {
                return loadData_present;
            }
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "continueDomainDef", IsSet = false)]
        public class ContinueDomainDefSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ContinueDomainDefSequenceType));
            private Identifier domainName_;


            private LoadData loadData_;

            [ASN1Element(Name = "domainName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public Identifier DomainName
            {
                get
                {
                    return domainName_;
                }
                set
                {
                    domainName_ = value;
                }
            }

            [ASN1Element(Name = "loadData", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public LoadData LoadData
            {
                get
                {
                    return loadData_;
                }
                set
                {
                    loadData_ = value;
                }
            }


            public void initWithDefaults()
            {
            }

            public IASN1PreparedElementData PreparedData
            {
                get
                {
                    return preparedData;
                }
            }
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "piDefinition", IsSet = false)]
        public class PiDefinitionSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(PiDefinitionSequenceType));
            private ICollection<Identifier> listOfDomains_;


            private bool monitorType_;

            private bool monitorType_present;


            private ProgramInvocationState pIState_;

            private bool pIState_present;
            private Identifier piName_;
            private bool reusable_;

            [ASN1Element(Name = "piName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
            public Identifier PiName
            {
                get
                {
                    return piName_;
                }
                set
                {
                    piName_ = value;
                }
            }

            [ASN1SequenceOf(Name = "listOfDomains", IsSetOf = false)]
            [ASN1Element(Name = "listOfDomains", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public ICollection<Identifier> ListOfDomains
            {
                get
                {
                    return listOfDomains_;
                }
                set
                {
                    listOfDomains_ = value;
                }
            }

            [ASN1Boolean(Name = "")]
            [ASN1Element(Name = "reusable", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = true)]
            public bool Reusable
            {
                get
                {
                    return reusable_;
                }
                set
                {
                    reusable_ = value;
                }
            }

            [ASN1Boolean(Name = "")]
            [ASN1Element(Name = "monitorType", IsOptional = true, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public bool MonitorType
            {
                get
                {
                    return monitorType_;
                }
                set
                {
                    monitorType_ = value;
                    monitorType_present = true;
                }
            }

            [ASN1Element(Name = "pIState", IsOptional = true, HasTag = true, Tag = 4, HasDefaultValue = false)]
            public ProgramInvocationState PIState
            {
                get
                {
                    return pIState_;
                }
                set
                {
                    pIState_ = value;
                    pIState_present = true;
                }
            }


            public void initWithDefaults()
            {
                bool param_Reusable =
                    false;
                Reusable = param_Reusable;
            }

            public IASN1PreparedElementData PreparedData
            {
                get
                {
                    return preparedData;
                }
            }

            public bool isMonitorTypePresent()
            {
                return monitorType_present;
            }

            public bool isPIStatePresent()
            {
                return pIState_present;
            }
        }
    }
}