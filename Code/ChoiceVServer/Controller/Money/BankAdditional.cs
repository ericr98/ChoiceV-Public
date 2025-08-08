using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {

    public enum BankAccountType : int {
        GiroKonto = 0,
        DepositKonto = 1,
        CompanyKonto = 2,
    }

    public enum BankAccountOwnerType : int {
        Player = 0,
        Company = 1,
        Controller = 2,
    }

    public enum RewardReturnValue : int {
        NoEffect = 0,
        HadEffect = 1,
        Runout = 2,
    }

    public class Bank {
        public static List<Type> AllRewardTypes = new List<Type>();

        public int Id { get; private set; }
        public string Name { get; private set; }
        public BankCompanies BankType { get; private set; }
        public int PreAccountNumber { get; private set; }
        public long BankPhoneNumber { get; private set; }

        public Dictionary<BankAccountType, float> AccountCountMap;

        public Bank(BankCompanies type, string name, long bankAccountNumber, int preAccountNumber) {
            Id = (int)type;
            Name = name;
            BankType = type;
            PreAccountNumber = preAccountNumber;
            BankPhoneNumber = bankAccountNumber;
            AccountCountMap = [];
            ((BankAccountType[])Enum.GetValues(typeof(BankAccountType))).ForEach(k => AccountCountMap[k] = 0);
        }

        public List<AccountReward> getCurrentRewards(BankAccountType type, List<Bank> allBanks) {
            var returnList = new List<AccountReward>();
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach(var item in allTypes) {
                if(item.GetInterfaces().Contains(typeof(IAccountRewardCreatable))) {
                    var arr = new object[] { type, allBanks.Where(b => b != this).ToList(), this };
                    var reward = (AccountReward)item.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, arr);
                    if(reward != null) {
                        returnList.Add(reward);
                    }
                }
            }

            return returnList;
        }

        public void addKonto(BankAccountType type) {
            lock(AccountCountMap) {
                var numb = AccountCountMap[type];
                numb++;
                AccountCountMap[type] = numb;
            }
        }
    }

    public delegate void AccountDataValueChangedDelegate(AccountReward reward);

    public class AccountReward {
        public static AccountDataValueChangedDelegate AccountDataValueChangedDelegate;

        public int Id { get; private set; }
        public ExtendedDictionary<string, dynamic> Data { get; private set; }

        public AccountReward(bankaccountreward dbReward) {
            Id = dbReward.id;
            Data = new ExtendedDictionary<string, dynamic>(dbReward.data.FromJson<Dictionary<string, dynamic>>());
            Data.OnValueChanged += onValueChange;
        }

        public AccountReward() {
            Id = -1;
            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
            Data.OnValueChanged += onValueChange;
        }

        private void onValueChange(string key, dynamic value) {
            AccountDataValueChangedDelegate?.Invoke(this);
        }

        public virtual RewardReturnValue takeEffectOnAccount(bankaccount dbAccount) { return RewardReturnValue.NoEffect; }

        public virtual RewardReturnValue takeEffectOnTransaction(banktransaction dbTransaction) { return RewardReturnValue.NoEffect; }

        public virtual RewardReturnValue takeEffectOnWithdraw(bankatmwithdraw dbWithdraw) { return RewardReturnValue.NoEffect; }

        public virtual RewardReturnValue takeEffectOnInterest(bankaccountinterest dbInterest) { return RewardReturnValue.NoEffect; }

        public virtual StaticMenuItem getMenuItemRepresentative() { return new StaticMenuItem("", "", ""); }
    }

    /// <summary>
    /// All AccountRewards must implement this interface.
    /// All AccountRewards must contain a Create function with these parameters:
    /// (BankAccountTypes type, List<Bank> AllBanks, Bank selectedBank)
    /// </summary>
    public interface IAccountRewardCreatable { }

    public class BalanceAccountReward : AccountReward, IAccountRewardCreatable {
        private static readonly decimal BalanceAccountRewardMax = 2500;

        public decimal BalanceAmount { get => (decimal)Data["BalanceAmount"]; set => Data["BalanceAmount"] = value; }

        public BalanceAccountReward(bankaccountreward dbReward) : base(dbReward) { }

        public BalanceAccountReward(decimal balance) : base() {
            BalanceAmount = balance;
        }

        public override RewardReturnValue takeEffectOnAccount(bankaccount dbAccount) {
            dbAccount.balance += BalanceAmount;
            return RewardReturnValue.Runout;
        }

        public static BalanceAccountReward Create(BankAccountType type, List<Bank> AllBanks, Bank selectedBank) {
            var bestPercent = float.MaxValue;
            foreach(var bank in AllBanks) {
                if(bank != selectedBank) {
                    var i = (selectedBank.AccountCountMap[type] + 1) / (bank.AccountCountMap[type] + 1);
                    if(bestPercent > i) {
                        bestPercent = i;
                    }
                }
            }

            var balance = excecuteFunct(bestPercent);

            if(balance > 0) {
                return new BalanceAccountReward(balance);
            } else {
                return null;
            }
        }

        private static decimal excecuteFunct(float percent) {
            //function is -x + 1
            if(percent <= 0) {
                return BalanceAccountRewardMax;
            } else {
                var y = Convert.ToDecimal((-1) * Math.Pow(percent, 1) + 1);
                if(y < 0) {
                    return 0;
                } else {
                    return Math.Round(BalanceAccountRewardMax * y, 2);
                }
            }
        }

        public override StaticMenuItem getMenuItemRepresentative() {
            return new StaticMenuItem("Startguthaben", $"Erhalte ein Startguthaben von ${BalanceAmount}", $"${BalanceAmount}");
        }
    }

    public class DepositInterestPercentReward : AccountReward, IAccountRewardCreatable {
        private static readonly float DepositInterestPercentRewardMax = 0.1f;
        private static readonly TimeSpan DepositInterestDurationRewardMax = TimeSpan.FromDays(30);

        public float InterestPercent { get => (float)Data["InterestPercent"]; set => Data["InterestPercent"] = value; }
        public DateTime InterestDuration { get => (DateTime)Data["InterestDuration"]; set => Data["InterestDuration"] = value; }

        public DepositInterestPercentReward(bankaccountreward dbReward) : base(dbReward) { }

        public DepositInterestPercentReward(float interestPercent, DateTime interestDuration) : base() {
            InterestPercent = interestPercent;
            InterestDuration = interestDuration;
        }

        public static DepositInterestPercentReward Create(BankAccountType type, List<Bank> AllBanks, Bank selectedBank) {
            if(type != BankAccountType.DepositKonto) {
                return null;
            }

            var bestPercent = float.MaxValue;
            foreach(var bank in AllBanks) {
                if(bank != selectedBank) {
                    var i = (selectedBank.AccountCountMap[type] + 1) / (bank.AccountCountMap[type] + 1);
                    if(bestPercent > i) {
                        bestPercent = i;
                    }
                }
            }

            var interestPer = excecuteFunct(bestPercent, DepositInterestPercentRewardMax);
            var interestDur = TimeSpan.FromMilliseconds(excecuteFunct(bestPercent, (float)DepositInterestDurationRewardMax.TotalMilliseconds));

            if(interestPer > 0) {
                return new DepositInterestPercentReward(interestPer, DateTime.Now + interestDur);
            } else {
                return null;
            }
        }

        private static float excecuteFunct(float percent, float max) {
            //function is -x + 1
            if(percent <= 0) {
                return max;
            } else {
                var y = (-1) * (float)Math.Pow(percent, 1) + 1;
                if(y < 0) {
                    return 0;
                } else {
                    return max * y;
                }
            }
        }

        public override RewardReturnValue takeEffectOnInterest(bankaccountinterest dbInterest) {
            if(InterestDuration <= DateTime.Now) {
                dbInterest.interestPercent = Constants.BANK_DEPOSIT_INTEREST_PERCENT;
                return RewardReturnValue.Runout;
            } else {
                dbInterest.interestPercent = Constants.BANK_DEPOSIT_INTEREST_PERCENT + InterestPercent;
                return RewardReturnValue.HadEffect;
            }
        }

        public override StaticMenuItem getMenuItemRepresentative() {
            return new StaticMenuItem("Zinssatzbonus", $"Einen um {Math.Round(InterestPercent * 100, 1)}% größeren Zinssatz bis {InterestDuration.Date.ToString("m", CultureInfo.CreateSpecificCulture("de-DE"))}", $"{Math.Round(InterestPercent * 100, 1)}% für {Math.Round((InterestDuration - DateTime.Now).TotalDays, 0)} Tage");
        }
    }

}
