-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev
-- ------------------------------------------------------
-- Server version	8.0.25

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `configweapons`
--

DROP TABLE IF EXISTS `configweapons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configweapons` (
  `weaponName` varchar(45) NOT NULL,
  `weaponType` varchar(45) DEFAULT NULL,
  `displayName` varchar(45) DEFAULT NULL,
  `displayType` varchar(45) DEFAULT NULL,
  `attachObjectPosition` varchar(45) DEFAULT NULL,
  `attachObjectRotation` varchar(45) DEFAULT NULL,
  `attachObjectBone` int DEFAULT NULL,
  `damageMult` float NOT NULL DEFAULT '1',
  `attachObjectModel` varchar(45) DEFAULT NULL,
  `equipSlot` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`weaponName`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configweapons`
--

LOCK TABLES `configweapons` WRITE;
/*!40000 ALTER TABLE `configweapons` DISABLE KEYS */;
INSERT INTO `configweapons` VALUES ('WEAPON_ASSAULTRIFLE','Rifle',NULL,'Automatikgewehr','{\"X\":0.020000007,\"Y\":-0.17000002,\"Z\":-0.02}','{\"X\":0,\"Y\":-54,\"Z\":0}',24818,1,'w_ar_assaultrifle','longWeapon'),('WEAPON_BAT','Knife','Baseballschläger','Nahkampfwaffe',NULL,NULL,NULL,0.6,NULL,'melee'),('WEAPON_BULLPUPRIFLE','Rifle',NULL,'Automatikgewehr','{\"X\":0.020000007,\"Y\":-0.17000002,\"Z\":-0.02}','{\"X\":0,\"Y\":-54,\"Z\":0}',24818,0.3,'w_ar_bullpuprifle','longWeapon'),('WEAPON_CARBINERIFLE','Rifle',NULL,'Automatikgewehr','{\"X\":0.020000007,\"Y\":-0.17000002,\"Z\":-0.02}','{\"X\":0,\"Y\":120,\"Z\":0}',24818,0.3,'w_ar_specialcarbine','longWeapon'),('WEAPON_COMBATPISTOL','Pistol','Hawk & Little: 1000er','Pistole',NULL,NULL,NULL,0.75,NULL,'pistol'),('WEAPON_DAGGER','Knife','Antiker Kavalleriedolch','Nahkampfwaffe',NULL,NULL,NULL,1,NULL,'melee'),('WEAPON_DOUBLEACTION','Pistol','D.D.P: Bolt M24','Pistole',NULL,NULL,NULL,0.66,NULL,'pistol'),('WEAPON_FLASHLIGHT','Knife','Taschenlampe','Nahkampfwaffe',NULL,NULL,NULL,0.6,NULL,'melee'),('WEAPON_KNIFE','Knife','Messer','Nahkampfwaffe',NULL,NULL,NULL,0.4,NULL,'melee'),('WEAPON_MARKSMANRIFLE','Sniper',NULL,'Präzisionsgewehr','{\"X\":0.020000007,\"Y\":-0.17000002,\"Z\":-0.02}','{\"X\":0,\"Y\":120,\"Z\":0}',24818,0.3,'w_sr_marksmanrifle','longWeapon'),('WEAPON_MUSKET','AntiqueMusket','Antike Muskete','Antike Muskete','{\"X\":-0.06,\"Y\":-0.17000002,\"Z\": 0.15}','{\"X\":0,\"Y\":120,\"Z\":0}',24818,0.3,'w_ar_musket','longWeapon'),('WEAPON_NIGHTSTICK','Knife','Züchtigungsknüppel','Nahkampfwaffe',NULL,NULL,NULL,0.5,NULL,'melee'),('WEAPON_PISTOL','Pistol','Hawk & Little: Terebba 2310','Pistole',NULL,NULL,NULL,1,NULL,'pistol'),('WEAPON_PISTOL50','Pistol','Hawk & Little: Jungle Puma','Pistole',NULL,NULL,NULL,0.625,NULL,'pistol'),('WEAPON_PISTOL_MK2','Pistol','Hawk & Little: Knock 13','Pistole',NULL,NULL,NULL,0.1,NULL,'pistol'),('WEAPON_SMG','Smg','Hawk & Little: SMG','Smg','{\"X\":0.060000002,\"Y\":-0.17000002,\"Z\":0.02}','{\"X\":0,\"Y\":120,\"Z\":0}',24818,0.5,'w_sb_smg','longWeapon'),('WEAPON_SPECIALCARBINE','Rifle',NULL,'Automatikgewehr','{\"X\":0.020000007,\"Y\":-0.17000002,\"Z\":-0.02}','{\"X\":0,\"Y\":120,\"Z\":0}',24818,0.5,'w_ar_carbinerifle','longWeapon'),('WEAPON_STONE_HATCHET','Knife','Antikes Chumash-Beil','Nahkampfwaffe',NULL,NULL,NULL,1,NULL,'melee'),('WEAPON_STUNGUN','Pistol','Coil: Stun Gun','Pistole',NULL,NULL,NULL,0.1,NULL,'pistol'),('WEAPON_SWITCHBLADE','Knife','Klappmesser','Nahkampfwaffe',NULL,NULL,NULL,0.2,NULL,'melee');
/*!40000 ALTER TABLE `configweapons` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:19
