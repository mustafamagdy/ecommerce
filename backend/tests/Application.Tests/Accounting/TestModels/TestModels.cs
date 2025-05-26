using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Accounting.Enums;
using FSH.WebApi.Domain.Common.Contracts;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Tests.Accounting.TestModels
{
    // Test models that can be used for mocking to avoid reflection in tests
    
    public class TestAccount : Account
    {
        public TestAccount(string accountNumber, string accountName, AccountType accountType, decimal balance = 0)
            : base(accountNumber, accountName, accountType, balance)
        {
        }

        // Add settable properties for testing
        public new bool IsActive { get; set; } = true;
        public new Guid Id { get; set; }
    }

    public class TestJournalEntry : JournalEntry
    {
        public TestJournalEntry(DateTime entryDate, string description)
            : base(entryDate, description)
        {
        }

        // Add settable properties for testing
        public new JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
        public new Guid Id { get; set; }
        
        // Add a method to add transactions for testing
        public new void AddTransaction(Transaction transaction)
        {
            base.AddTransaction(transaction);
        }
    }
} 