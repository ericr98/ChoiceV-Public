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
-- Table structure for table `configobjects`
--

DROP TABLE IF EXISTS `configobjects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configobjects` (
  `id` int NOT NULL AUTO_INCREMENT,
  `modelName` varchar(255) DEFAULT NULL,
  `modelHash` varchar(255) DEFAULT NULL,
  `codeFunctionOrIdentifier` varchar(255) NOT NULL,
  `info` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=90 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configobjects`
--

LOCK TABLES `configobjects` WRITE;
/*!40000 ALTER TABLE `configobjects` DISABLE KEYS */;
INSERT INTO `configobjects` VALUES (1,NULL,'3640564381','OPEN_SNACK_SHOP','Snackautomat'),(3,'prop_parknmeter_01',NULL,'onParkClockInteraction',NULL),(4,'prop_parknmeter_02',NULL,'onParkClockInteraction',NULL),(6,'prop_atm_02',NULL,'onATMInteraction','Atm'),(7,'prop_atm_03',NULL,'onATMInteraction','Atm'),(8,'prop_fleeca_atm',NULL,'onATMInteraction','Fleeca Atm'),(12,'prop_printer_01',NULL,'ON_PRINTER_INTERACT',NULL),(13,'prop_printer_02',NULL,'ON_PRINTER_INTERACT',NULL),(15,'prop_dumpster_02a',NULL,'OPEN_TRASHCAN','Müll'),(16,'prop_sglasses_stand_02',NULL,'OPEN_SUNGLASSES_SHOP','Sonnebrillenständer'),(17,'prop_sglasses_stand_02b',NULL,'OPEN_SUNGLASSES_SHOP','Sonnebrillenständer'),(18,'prop_sglasses_stand_03',NULL,'OPEN_SUNGLASSES_SHOP','Sonnebrillenständer'),(19,'prop_sglasses_stand_01',NULL,'OPEN_SUNGLASSES_SHOP','Sonnebrillenständer'),(20,'prop_sglasses_stand_1b',NULL,'OPEN_SUNGLASSES_SHOP','Sonnebrillenständer'),(21,'prop_display_unit_01',NULL,'OPEN_HAT_SHOP','Hutdisplay'),(22,'prop_dumpster_01a',NULL,'OPEN_TRASHCAN','Dumpster'),(23,'prop_bin_01a',NULL,'OPEN_TRASHCAN','Mülleimer'),(24,'prop_bin_07a',NULL,'OPEN_TRASHCAN','Mülleimer'),(25,'prop_bin_07c',NULL,'OPEN_TRASHCAN','Mülleimer'),(26,'prop_bin_08a',NULL,'OPEN_TRASHCAN','Mülleimer'),(27,'prop_dumpster_02b',NULL,'OPEN_TRASHCAN','Mülleimer'),(29,'prop_monitor_w_large',NULL,'MONITOR_INTERACT','Monitor'),(30,'prop_monitor_01a',NULL,'MONITOR_INTERACT','Monitor'),(31,'v_res_lest_monitor',NULL,'MONITOR_INTERACT','Monitor'),(32,'v_corp_officedesk',NULL,'MONITOR_INTERACT','Bürotisch'),(34,NULL,'4286933769','MONITOR_INTERACT','einzelner Monitor weißer Bildschirm'),(35,NULL,'881450200','MONITOR_INTERACT','Laptop Fenster offen'),(36,NULL,'1845693979','MONITOR_INTERACT','Tisch mit 3 Monitoren'),(37,NULL,'1230813074','MONITOR_INTERACT','Schreibtisch mit altem Monitor'),(38,NULL,'1140820728','MONITOR_INTERACT','alter Monitor'),(39,NULL,'2109346928','MONITOR_INTERACT','Laptop schwarz'),(40,NULL,'1927761070','MONITOR_INTERACT','schwarzer Flachbildschirm'),(41,NULL,'992647982','MONITOR_INTERACT','Desk mit zwei Monitoren'),(42,NULL,'396006926','MONITOR_INTERACT','Schwarzer Flachbildschirm'),(43,NULL,'1385417869','MONITOR_INTERACT','Laptop iFruit'),(44,NULL,'3853585930','MONITOR_INTERACT','Schreibtisch zwei Bildschirme'),(45,NULL,'3373913304','MONITOR_INTERACT','Schreibtisch ein Monitor'),(46,NULL,'1940636184','MONITOR_INTERACT','PC AoD'),(47,NULL,'758202391','MONITOR_INTERACT','Laptop silber'),(48,NULL,'2922097788','MONITOR_INTERACT','iFruit groß'),(49,NULL,'24763681','MONITOR_INTERACT','Zwei Monitore, schwarzer Schreibtisch'),(50,NULL,'1476780484','MONITOR_INTERACT','Monitore doppelt'),(51,NULL,'2084153992','MONITOR_INTERACT','PC 3fach'),(52,NULL,'829413118','MONITOR_INTERACT','Monitor einzeln schwarz'),(53,NULL,'3566110672','MONITOR_INTERACT','Schreibtisch groß'),(54,NULL,'843005760','MONITOR_INTERACT','schwarzer kleiner Bildschirm'),(55,NULL,'2432808086','MONITOR_INTERACT','Desk mit Obst'),(56,NULL,'4290018809','MONITOR_INTERACT','Laptop grauer Tisch'),(57,NULL,'810004487','MONITOR_INTERACT','Monitor Bank Paleto'),(58,'prop_vend_soda_01',NULL,'OPEN_DRINK_SHOP','Cola-Maschine'),(59,'prop_vend_soda_02',NULL,'OPEN_DRINK_SHOP','Sprunk-Maschine'),(60,'prop_vend_water_01',NULL,'OPEN_WATER_SHOP','Water-Maschine'),(61,'prop_vend_coffe_01',NULL,'OPEN_COFFEE_SHOP','Kaffee-Maschine'),(62,'prop_vend_snak_01_tu','3260933171','OPEN_SNACK_SHOP','Snackautomat 2'),(63,'p_ld_coffee_vend_01',NULL,'OPEN_COFFEE_SHOP','Kaffee-Cart'),(64,'prop_copier_01','','ON_PRINTER_INTERACT','Kopierer groß'),(65,'prop_vend_fags_01',NULL,'OPEN_CIGARETTE_SHOP','Zigarettenautomat'),(66,NULL,'2798610344','MONITOR_INTERACT','Monitor'),(67,NULL,'3349503439','MONITOR_INTERACT','Laptop'),(68,NULL,'3216060441','MONITOR_INTERACT','Laptop'),(69,NULL,'363555755','MONITOR_INTERACT','Laptop'),(70,NULL,'2994267336','MONITOR_INTERACT','Laptop'),(71,NULL,'2000956101','MONITOR_INTERACT','Laptop'),(73,NULL,'3269716226','ON_PRINTER_INTERACT','großer Drucker'),(74,NULL,'4148614582','MONITOR_INTERACT','PC mit 3 Bildschirmen'),(75,NULL,'2430557065','MONITOR_INTERACT','Schreibtisch'),(76,NULL,'1184105518','MONITOR_INTERACT','Röhrenmonitor'),(77,NULL,'4220543854','ON_PRINTER_INTERACT','großer Drucker'),(80,'prop_printer_03',NULL,'ON_PRINTER_INTERACT','Drucker'),(86,'v_res_printer',NULL,'ON_PRINTER_INTERACT','kleiner Drucker'),(88,NULL,'275188277','OPEN_TRASHCAN','Mülleimer grau'),(89,NULL,'2799241286','MONITOR_INTERACT','Monitor');
/*!40000 ALTER TABLE `configobjects` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:43
