using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class CompanyIdCard : IdCardItem {
        private string Type;
        
        public CompanyIdCard(item item) : base(item) {
            EventName = "OPEN_COMPANY_ID_CARD";
        }

        public CompanyIdCard(configitem cfg, string icon, IPlayer player, Company company) : base(cfg) {
            EventName = "OPEN_COMPANY_ID_CARD";

            var cardNumber = "Nicht gefunden!";
            var rankName = "Nicht gefunden!";
            var rankHiringDate = "Nicht gefunden!";
            
            var charId = player.getCharacterId();
            using(var dbFs = new ChoiceVFsDb()) {
                var dbRank = dbFs.permission_ranks.Include(r => r.permission_users_to_ranks).FirstOrDefault(r => r.systemId == company.FsSystemId && r.permission_users_to_ranks.Any(u => u.userId == charId));
                var dbEmployee = dbFs.employees.FirstOrDefault(e => e.systemId == company.FsSystemId && e.charId == charId);
                
                if(dbRank != null) {
                    rankName = dbRank.rankName;
                }

                if(dbEmployee != null) {
                    cardNumber = dbEmployee.memberNumber;
                    rankHiringDate = dbEmployee.createDate;
                }
            } 
            
            setData([
                new IdCardItemElement("type", cfg.additionalInfo),
                new IdCardItemElement("icon", icon),
                new IdCardItemElement("name", player.getCharacterShortenedName()),
                new IdCardItemElement("birthday", player.getCharacterData().DateOfBirth.ToString("yyyy-MM-dd")),
                new IdCardItemElement("number", cardNumber),
                new IdCardItemElement("hiringDay", rankHiringDate),
                new IdCardItemElement("rank", rankName),
                new IdCardItemElement("company", company.Name),
                new IdCardItemElement("address", company.StreetName),
            ]);
            
            Description = $"{player.getCharacterShortenedName()} von {company.Name} als {rankName}";
        }
    }
}