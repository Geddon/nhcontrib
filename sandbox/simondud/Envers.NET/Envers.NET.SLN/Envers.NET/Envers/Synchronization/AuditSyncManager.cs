﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Envers.RevisionInfo;
using NHibernate.Event;
using System.Collections;

namespace NHibernate.Envers.Synchronization
{
    /**
     * @author Simon Duduica, port of Envers omonyme class by Adam Warski (adam at warski dot org)
     */
    public class AuditSyncManager {
        private readonly IDictionary<ITransaction, AuditSync> auditSyncs;
        private readonly IRevisionInfoGenerator revisionInfoGenerator;

        public AuditSyncManager(IRevisionInfoGenerator revisionInfoGenerator) {
            //ORIG: auditSyncs = new ConcurrentHashMap<ITransaction, AuditSync>(); TODO Simon see if it's OK
            auditSyncs = new Dictionary<ITransaction, AuditSync>();

            this.revisionInfoGenerator = revisionInfoGenerator;
        }

        public AuditSync get(IEventSource session) {
            ITransaction transaction = session.Transaction;

            AuditSync verSync = auditSyncs[transaction];
            if (verSync == null) {
                // No worries about registering a transaction twice - a transaction is single thread
                verSync = new AuditSync(this, session, revisionInfoGenerator);
                auditSyncs.Add(transaction, verSync);

                transaction.RegisterSynchronization(verSync);
            }

            return verSync;
        }

        public void Remove(ITransaction transaction) {
            auditSyncs.Remove(transaction);
        }
    }
}