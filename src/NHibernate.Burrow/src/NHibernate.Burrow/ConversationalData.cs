using System;
using NHibernate.Burrow.DataContainers;
using NHibernate.Burrow.Exceptions;
using NHibernate.Burrow.Exceptions;

namespace NHibernate.Burrow {
    public enum ConversationalDataMode {
        Single,
        SingleTemp,
        /// <summary>
        /// overspan mulitple conversations
        /// </summary>
        Overspan
    }

    /// <summary>
    /// A Data container for conversational data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ConversationalData<T> {
        private Guid gid = Guid.Empty;
        private ConversationalDataMode mode = ConversationalDataMode.SingleTemp;
        public ConversationalData() {}

        /// <summary>
        /// 
        /// </summary>
        public ConversationalData(ConversationalDataMode mode) {
            this.mode = mode;
        }

        public ConversationalData(ConversationalDataMode mode, T value) {
            this.mode = mode;
            Value = value;
        }

        public ConversationalDataMode Mode {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Gets and sets the data value of this container
        /// </summary>
        public T Value {
            get {
                T retVal = default(T);
                if (gid == Guid.Empty)
                    return retVal;
                if (!cItems.TryGet(gid, out retVal)) {
                    if (Mode == ConversationalDataMode.Single)
                        throw new ConversationUnavailableException(
                            "Conversation may have changed, if you don't need to keep data after conversation changed, please use SingleTemp Mode on.");
                    if (Mode == ConversationalDataMode.SingleTemp)
                        gid = Guid.Empty;
                }
                return retVal;
            }
            set {
                if (Equals(value, default(T))) {
                    if (gid != Guid.Empty)
                        cItems.Remove(gid);
                    gid = Guid.Empty;
                }
                else if (gid == Guid.Empty)
                    gid = cItems.CreateSlot(value);
                else
                    cItems.Set(gid, value);
            }
        }

        /// <summary>
        /// indicates if this data is out of conversation 
        /// </summary>
        public bool OutOfConversation {
            get { return gid != Guid.Empty && !cItems.ContainsKey(gid); }
        }

        private static GuidDataContainer cItems {
            get {
                if (Conversation.Current == null) {
                    string msg = "ConversationalData can not be accessed outside conversation. " +
                                 "Make sure Conversation is intialized before visiting conversational data."
                                 +
                                 " It might be caused by missing <add name=\"NHSessModule\" type=\"NHibernate.Burrow.NHDomain.NHSessHTTPModule\" /> in the Web.Config file";
                    throw new ConversationUnavailableException(
                        msg);
                }
                return Conversation.Current.Items;
            }
        }
    }
}