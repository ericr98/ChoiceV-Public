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
-- Table structure for table `configcrimehoboquestions`
--

DROP TABLE IF EXISTS `configcrimehoboquestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configcrimehoboquestions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codeType` varchar(45) NOT NULL,
  `pillarId` int DEFAULT NULL,
  `name` varchar(55) NOT NULL,
  `labels` text NOT NULL,
  `requiredReputation` int NOT NULL,
  `settings` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configcrimehoboquestions`
--

LOCK TABLES `configcrimehoboquestions` WRITE;
/*!40000 ALTER TABLE `configcrimehoboquestions` DISABLE KEYS */;
INSERT INTO `configcrimehoboquestions` VALUES (7,'HoboCrimeInformationQuestion',NULL,'Wie kann ich hier Geld verdienen?','[\"Start\",\"Anfang\",\"Geld\",\"Arbeit\",\"Kohle\",\"Money\",\"Wie\",\"Was\",\"Job\"]',0,'{\"Information\":\"Kommt drauf an, was du machen willst. Es gibt viele Wege, um easy Kohle zu machen. Du kannst Zeugs klauen, Drogen verticken oder dich der Hehlerei widmen.\"}'),(8,'HoboCrimeModuleInShiftSearchQuestion',1,'Wo finde ich einen Dealer-Auftraggeber?','[\"Auftrag\",\"Aufträge\",\"Auftraggeber\",\"Arbeitgeber\",\"Dealerauftrag\"]',0,'{\"ModuleName\":\"CrimeMissionModule\"}'),(9,'HoboCrimeModuleInShiftSearchQuestion',2,'Wo finde ich einen Hehler-Auftraggeber?','[\"Auftrag\",\"Aufträge\",\"Auftraggeber\",\"Arbeitgeber\",\"Hehlerauftrag\"]',0,'{\"ModuleName\":\"CrimeMissionModule\"}'),(10,'HoboCrimeModuleInShiftSearchQuestion',0,'Wo finde ich einen Einbrecher-Auftraggeber?','[\"Auftrag\",\"Aufträge\",\"Auftraggeber\",\"Arbeitgeber\",\"Einbrecherauftrag\"]',0,'{\"ModuleName\":\"CrimeMissionModule\"}'),(11,'HoboCrimeInformationQuestion',1,'Wie kann ich Drogendealer werden?','[\"Drogen\",\"Dealer\",\"dealen\",\"Substanzen\"]',0,'{\"Information\":\"Sprich einen der Auftraggeber, die zu den Dealern gehören, an und frag sie nach Arbeit. Ich kann dir auch sagen, wer gerade von denen am Start ist, wenn du das wissen willst.\"}'),(12,'HoboCrimeInformationQuestion',0,'Wie kann ich Einbrecher werden?','[\"klauen\",\"einbrechen\",\"besorgen\",\"stehlen\",\"Diebstahl\",\"Einbrecher\",\"Einbruch\"]',0,'{\"Information\":\"Sprich einen der Auftraggeber, die zu den Einbrechern gehören, an und frag sie nach Arbeit. Ich kann dir auch sagen, wer gerade von denen am Start ist, wenn du das wissen willst.\"}'),(13,'HoboCrimeInformationQuestion',2,'Wie kann ich Hehler werden?','[\"Diebesgut\",\"Beute\",\"verkaufen\",\"Hehlerei\",\"Hehler\",\"loswerden\"]',0,'{\"Information\":\"Sprich einen der Auftraggeber, die zu den Hehlern gehören, an und frag sie nach Arbeit. Ich kann dir auch sagen, wer gerade von denen am Start ist, wenn du das wissen willst.\"}'),(14,'HoboCrimeInformationQuestion',1,'Wo finde ich diesen Hotdogtypen?','[\"Hotdogtyp\",\"Hotdogtypen\",\"Hotdog-Typ\",\"Hotdog-Typen\"]',0,'{\"Information\":\"Der steht normalerweise an seinem Hotdogstand an der Promenade in der Nähe vom Muscle Beach. In der Nähe von so nem Burgertypen. Die streiten sich da ständig.\"}'),(15,'HoboCrimeInformationQuestion',2,'Wo genau krieg ich Hehlerware her?','[\"Hehlerware\"]',0,'{\"Information\":\"Ich empfehle dir, dich mit Leuten zu connecten, die professionell sowas beschaffen und irgendwie wieder loswerden wollen, um langfristig das gute Zeug zu bekommen. Wenn du grade erst anfängst, gibts da immer mal wieder ein paar Gauner, die bissel Zeugs für nen kleinen Preis anbieten.\"}'),(17,'HoboCrimeModuleInShiftSearchQuestion',2,'Wo finde ich einen von diesen Gaunern?','[\"Gauner\"]',0,'{\"ModuleName\":\"BaseTraderModule\"}'),(18,'HoboCrimeItemBuyShopWithItemsQuestion',2,'Wo verkaufe ich das?','[\"Ohrringe\",\"Halskette\",\"Ring\",\"Silbernes Armband\",\"Verzierter Handspiegel\"]',0,'{\"ConfigItemIds\":\"[622,623,624,625,626]\"}'),(19,'HoboCrimeInformationQuestion',1,'Wo krieg ich diese Droge her?','[\"Joint\",\"Hasch-Joint\",\"Charas-Joint\",\"Kokain\"]',0,'{\"Information\":\"Schau dich auf der Straße um, da findest du normalerweise Leute, die dir was verkaufen. Fürs Erste zumindest…\"}'),(20,'HoboCrimeInformationQuestion',1,'Wo kann ich diese Drogen verkaufen?','[\"Drogenverkauf\",\"Abnehmer\",\"Verkauf\",\"Drogenabnehmer\"]',0,'{\"Information\":\"Da gibts mehrere Möglichkeiten. Entweder du quatscht random Leute auf der Straße an oder für die Aufträge gehst du fürs Erste zum Hotdogtypen.\"}'),(21,'HoboCrimeModuleInShiftSearchQuestion',1,'Wo isn der Hotdogtypi','[\"Testhotdog\"]',0,'{\"ModuleName\":\"BaseTraderModule\"}');
/*!40000 ALTER TABLE `configcrimehoboquestions` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:14
