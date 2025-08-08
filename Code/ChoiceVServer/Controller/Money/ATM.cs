using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Money {
    public delegate void ATMCashChangeDelegate(ATM atm);
    public delegate void ATMInteractionDelegate(ATM atm, IPlayer player);

    public class ATM {
        public static ATMCashChangeDelegate ATMCashChangeDelegate;
        public static ATMInteractionDelegate ATMInteractionDelegate;

        public int Id { get; private set; }
        public string Name { get; private set; }
        public CollisionShape CollisionShape { get; private set; }
        public BankCompanies Company { get; }

        public decimal Deposit { get; private set; }

        public DateTime LastHeistDate;

        public decimal HackHeistOutput;

        public ATM(int id, string name, Position position, float width, float height, float rotation, BankCompanies company, decimal deposit, DateTime lastHeistDate) {
            Id = id;
            Name = name;
            CollisionShape = CollisionShape.Create(position, width, height, rotation, true, false, true, "ATM_INTERACTION", new Dictionary<string, dynamic> { { "Atm", this } });
            CollisionShape.OnCollisionShapeInteraction += onInteraction;
            Company = company;

            Deposit = deposit;
            LastHeistDate = lastHeistDate;
        }

        private bool onInteraction(IPlayer player) {
            ATMInteractionDelegate?.Invoke(this, player);
            return true;
        }

        public decimal withdrawCash(decimal wantedAmount) {
            if (Deposit > wantedAmount) {
                Deposit -= wantedAmount;
                ATMCashChangeDelegate?.Invoke(this);
                return wantedAmount;
            } else {
                var temp = Deposit;
                Deposit = 0;
                ATMCashChangeDelegate?.Invoke(this);
                return temp;
            }
        }


        public void refill(decimal amount) {
            Deposit += amount;
            ATMCashChangeDelegate?.Invoke(this);
        }

        public void remove() {
            CollisionShape.Dispose();
        }
    }
}
