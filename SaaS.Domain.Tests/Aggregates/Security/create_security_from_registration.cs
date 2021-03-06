#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using NUnit.Framework;
using Sample;

// ReSharper disable InconsistentNaming

namespace SaaS.Aggregates.Security
{
    public class create_security_from_registration : security_syntax
    {
        static readonly SecurityId id = new SecurityId(42);
        static readonly RegistrationId reg = new RegistrationId(Guid.NewGuid());

        [Test]
        public void create_with_identity()
        {
            Given(Identity.SetNextId(12));
            When(new CreateSecurityFromRegistration(id, reg, "login", "pass", "display", "identity"));
            Expect(new SecurityAggregateCreated(id),
                new SecurityPasswordAdded(id, new UserId(12), "display", "login", "pass+salt", "salt",
                    "generated-32"),
                new SecurityIdentityAdded(id, new UserId(13), "display", "identity", "generated-32"),

                new SecurityRegistrationProcessCompleted(id, "display", new UserId(12), "generated-32", reg));

        }


        [Test]
        public void create_simple()
        {
            Given(Identity.SetNextId(12));
            When(new CreateSecurityFromRegistration(id, reg, "login", "pass", "display", null));
            Expect(new SecurityAggregateCreated(id),
                new SecurityPasswordAdded(id, new UserId(12), "display", "login", "pass+salt", "salt",
                    "generated-32"),
                new SecurityRegistrationProcessCompleted(id, "display", new UserId(12), "generated-32", reg));
        }
    }
}