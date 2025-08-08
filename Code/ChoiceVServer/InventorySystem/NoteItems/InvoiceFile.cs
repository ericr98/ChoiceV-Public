using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {

    public class InvoiceController : ChoiceVScript {
        public InvoiceController() {
            EventController.addMenuEvent("INVOICE_PAY_BAR", onPayInvoiceBar);
            EventController.addMenuEvent("INVOICE_PAY_KONTO", onPayInvoiceKonto);
            EventController.addMenuEvent("INVOICE_SHOW", onPayInvoiceShow);
        }

        public static int getNextInvoiceId(Company company) {
            using(var db = new ChoiceVDb()) {
                var dbComp = db.configcompanies.Find(company.Id);
                if(dbComp != null) {
                    dbComp.invoiceId++;
                    db.SaveChanges();
                    return dbComp.invoiceId ?? -1;
                } else {
                    return -1;
                }
            }
        }

        public static InvoiceFile createInvoice(Company company, List<InvoiceProduct> products) {
            var configItem = InventoryController.getConfigItemForType<InvoiceFile>();
            var file = new InvoiceFile(configItem, company);
            file.InvoiceProducts = products;
            file.updateDescription();
            return file;
        }

        private bool onPayInvoiceBar(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (InvoiceFile)data["Item"];

            if(item != null) {
                item.PaymentInfo = "bar";
                item.HasBeenPayed = true;
            } else {
                Logger.logError($"onPayInvoiceBar: Item not found",
                                    $"Fehler im Rechnungs-System: Zu bezahlende Rechnung konnte nicht gefunden werden", player);
            }

            return true;
        }

        private bool onPayInvoiceKonto(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (InvoiceFile)data["Item"];
            var company = CompanyController.findCompanyById((int)data["CompanyId"]);
            var account = (long)data["Konto"];

            if(item != null && company != null) {
                var playerBankAccounts = BankController.getPlayerBankAccounts(player);
                if(playerBankAccounts.All(p => p.id != account)) {
                    player.sendBlockNotification("Du hast keinen Zugriff auf dieses Konto!", "Kein Zugriff", Constants.NotifactionImages.ATM);
                    return false;
                }
                
                if(BankController.transferMoney(account, company.CompanyBankAccount, item.getCombindedPrice(), $"Bezahlung Rechnung: {item.FileId}", out var message, false)) {
                    player.sendNotification(Constants.NotifactionTypes.Success, "Rechnung erfolgreich bezahlt!", "Rechnung bezahlt", Constants.NotifactionImages.ATM);
                    item.PaymentInfo = $"Überweisung über {account}";
                    item.HasBeenPayed = true;
                } else {
                    player.sendBlockNotification($"Die Rechnung konnte nicht bezahlt werden: {message}", "Zu wenig Geld", Constants.NotifactionImages.ATM);
                }
            } else {
                Logger.logError($"onPayInvoiceBar: Item not found",
                                    $"Fehler im Rechnungs-System: Zu bezahlende Rechnung konnte nicht gefunden werden", player);
            }

            return true;
        }

        private bool onPayInvoiceShow(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (InvoiceFile)data["Item"];

            if(item != null) {
                var evt = new InvoiceFileCefEvent(item.CompanyName, item.FileId, item.CompanyCity, item.CompanyStreet, item.Tax, item.InvoiceProducts, item.Date, item.SignDate, item.PaymentInfo, item.AdditionalInfo, item.SellerSignature, item.BuyerSignature, item.IsCopy, item.Id ?? -1);
                player.emitCefEventWithBlock(evt, "CEF_FILE");
            } else {
                Logger.logError($"onPayInvoiceBar: Item not found",
                                    $"Fehler im Rechnungs-System: Zu zeigende Rechnung konnte nicht gefunden werden", player);
            }

            return true;
        }
    }

    public class InvoiceProduct {
        public int count;
        public decimal price;
        public string name;

        public InvoiceProduct(int count, decimal price, string name) {
            this.count = count;
            this.price = price;
            this.name = name;
        }
    }

    public class InvoiceFileCefEvent : IPlayerCefEvent {
        public string Event { get; private set; }

        public int invoiceId;

        public string companyName;
        public string street;
        public string city;

        public string signDate;
        public string date;
        public float tax;

        public string[] products;

        public string paymentInfo;
        public string additionalInfo;

        public string sellerSignature;
        public string buyerSignature;

        public bool isCopy;

        public int id;

        public InvoiceFileCefEvent() {
            Event = "OPEN_INVOICE_FILE";
        }

        public InvoiceFileCefEvent(string companyName, int invoiceId, string companyCity, string companyStreet, float tax, List<InvoiceProduct> products, string date, DateTime signDate, string paymentInfo, string additionalInfo, string sellerSignature, string buyerSignature, bool isCopy, int id) {
            Event = "OPEN_INVOICE_FILE";

            this.invoiceId = invoiceId;

            this.companyName = companyName;
            this.city = companyCity;
            this.street = companyStreet;

            this.signDate = signDate.ToString("dd.MM.yyyy");
            this.date = date;
            this.tax = tax;

            this.products = products.Select(p => p.ToJson()).ToArray();

            this.paymentInfo = paymentInfo;
            this.additionalInfo = additionalInfo;

            this.sellerSignature = sellerSignature;
            this.buyerSignature = buyerSignature;

            this.isCopy = isCopy;

            this.id = id;
        }

        public void populateItem(InvoiceFile item, IPlayer player) {
            item.Date = date;
            var split = signDate.Split(".");
            item.SignDate = new DateTime(int.Parse(split[2]), int.Parse(split[1]), int.Parse(split[0]));
            item.InvoiceProducts = products.Select(p => p.FromJson<InvoiceProduct>()).ToList();
            item.PaymentInfo = paymentInfo;
            item.AdditionalInfo = additionalInfo;
            if(sellerSignature == "TO_FILL") {
                item.SellerSignature = player.getCharacterShortName();
            }

            if(buyerSignature == "TO_FILL") {
                item.BuyerSignature = player.getCharacterShortName();
            }
        }
    }

    public class InvoiceFile : Item, File {
        public int CompanyId { get => (int)Data["CompanyId"]; set { Data["CompanyId"] = value; } }

        public int FileId { get => (int)Data["FileId"]; set { Data["FileId"] = value; } }

        public string CompanyName { get => (string)Data["CompanyName"]; set { Data["CompanyName"] = value; } }
        public string CompanyCity { get => (string)Data["CompanyCity"]; set { Data["CompanyCity"] = value; } }
        public string CompanyStreet { get => (string)Data["CompanyStreet"]; set { Data["CompanyStreet"] = value; } }

        public string Date { get => (string)Data["Date"]; set { Data["Date"] = value; } }
        public DateTime SignDate { get => (DateTime)Data["SignDate"]; set { Data["SignDate"] = value; } }

        public float Tax { get => (float)Data["Tax"]; set { Data["Tax"] = value; } }

        public string PaymentInfo { get => (string)Data["PaymentInfo"]; set { Data["PaymentInfo"] = value; } }
        public string AdditionalInfo { get => (string)Data["AdditionalInfo"]; set { Data["AdditionalInfo"] = value; } }

        public string SellerSignature { get => (string)Data["SellerSignature"]; set { Data["SellerSignature"] = value; } }
        public string BuyerSignature { get => (string)Data["BuyerSignature"]; set { Data["BuyerSignature"] = value; } }

        public bool HasBeenPayed { get => (bool)Data["HasBeenPayed"]; set { Data["HasBeenPayed"] = value; } }

        public bool IsCopy { get => (bool)Data["IsCopy"]; set { Data["IsCopy"] = value; } }

        public List<InvoiceProduct> InvoiceProducts { get => JsonConvert.DeserializeObject<List<InvoiceProduct>>(Data["InvoiceProducts"]); set { Data["InvoiceProducts"] = value.ToJson(); } }

        public InvoiceFile(item item) : base(item) { }

        public InvoiceFile(configitem configItem, Company company) : base(configItem) {
            CompanyId = company.Id;

            FileId = InvoiceController.getNextInvoiceId(company);

            CompanyName = company.Name;
            CompanyCity = company.CityName;
            CompanyStreet = company.StreetName;
            Tax = company.CompanyTax.TaxPercent;
            SignDate = DateTime.Now;

            Date = "";

            InvoiceProducts = new List<InvoiceProduct>();

            PaymentInfo = "Unbestimmt (Wird automatisch ausgefüllt)";
            AdditionalInfo = "";

            SellerSignature = "";
            BuyerSignature = "";

            HasBeenPayed = false;
            IsCopy = false;
            updateDescription();
        }

        public InvoiceFile(InvoiceFile file) : base(InventoryController.getConfigById(file.ConfigId)) {
            CompanyId = file.CompanyId;

            FileId = file.FileId;

            CompanyName = file.CompanyName;
            CompanyCity = file.CompanyCity;
            CompanyStreet = file.CompanyStreet;
            Tax = file.Tax;
            SignDate = file.SignDate;

            Date = file.Date;

            InvoiceProducts = file.InvoiceProducts;

            PaymentInfo = file.PaymentInfo;
            AdditionalInfo = file.AdditionalInfo;

            SellerSignature = file.SellerSignature;
            BuyerSignature = file.BuyerSignature;

            HasBeenPayed = file.HasBeenPayed;
            IsCopy = true;

            updateDescription();
        }

        public decimal getCombindedPrice() {
            decimal count = 0;

            foreach(var product in InvoiceProducts) {
                var combinded = product.price * product.count;
                count += combinded + (combinded * Convert.ToDecimal(Tax));
            }

            return decimal.Round(count, 2);
        }

        public void addInvoiceProduct(InvoiceProduct product) {
            InvoiceProducts.Add(product);
        }

        public override void use(IPlayer player) {
            base.use(player);

            if(SellerSignature != "" && BuyerSignature != "" && !HasBeenPayed && !IsCopy) {
                var menu = new Menu("Rechnungsmenü", "Was möchtest du tun?");

                var data = new Dictionary<string, dynamic> { { "Item", this }, { "CompanyId", CompanyId } };

                menu.addMenuItem(new ClickMenuItem("Bezahlung: Bar", "Bezahle die Rechnung Bar", "", "INVOICE_PAY_BAR").needsConfirmation("Bar Bezahlung eintragen?", "Die Rechnung wird als bar gekennzeichnet.").withData(data));
                menu.addMenuItem(new ClickMenuItem("Bezahlung: Überweisung", "Bezahle die Rechnung per Überweisung", "", "INVOICE_PAY_KONTO").withData(data.Concat(new Dictionary<string, dynamic> {{ "Konto", player.getMainBankAccount() }}).ToDictionary()));
                var companies = CompanyController.getCompaniesWithPermission(player, "PAY_WITH_BANK");
                foreach(var company in companies) {
                    var account = company.CompanyBankAccount;
                    menu.addMenuItem(new ClickMenuItem($"Bezahlung: Firmenkonto {company.ShortName}", $"Bezahle die Rechnung von deinem {account} Konto", "", "INVOICE_PAY_KONTO").withData(data.Concat(new Dictionary<string, dynamic> {{ "Konto", account }}).ToDictionary()));
                }
                
                
                menu.addMenuItem(new ClickMenuItem("Ansehen", "Sieh dir die Rechnung an", "", "INVOICE_SHOW").withData(data));
                player.showMenu(menu);
            } else {
                player.emitCefEventWithBlock(new InvoiceFileCefEvent(CompanyName, FileId, CompanyCity, CompanyStreet, Tax, InvoiceProducts, Date, SignDate, PaymentInfo, AdditionalInfo, SellerSignature, BuyerSignature, IsCopy, Id ?? -1), "CEF_FILE");
            }
        }

        public override void updateDescription() {
            if(InvoiceProducts.Count > 0) {
                Description = $"{(IsCopy ? "KOPIE: " : "")} Firma: {CompanyName}, Id: {FileId}, Preis: ${getCombindedPrice()}";
            } else {
                Description = $"{(IsCopy ? "KOPIE: " : "")} Firma: {CompanyName}, Id: {FileId}, Leer";
            }

            base.updateDescription();
        }

        public Item getCopy() {
            return new InvoiceFile(this);
        }
    }
}
