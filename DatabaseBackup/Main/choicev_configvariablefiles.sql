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
-- Table structure for table `configvariablefiles`
--

DROP TABLE IF EXISTS `configvariablefiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configvariablefiles` (
  `identifer` varchar(50) NOT NULL,
  `backgroundImage` varchar(200) NOT NULL,
  `width` float NOT NULL,
  `height` float NOT NULL,
  PRIMARY KEY (`identifer`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configvariablefiles`
--

LOCK TABLES `configvariablefiles` WRITE;
/*!40000 ALTER TABLE `configvariablefiles` DISABLE KEYS */;
INSERT INTO `configvariablefiles` VALUES ('ACLS_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/ACLS_ARBEITSVERTRAG.jpg',58,80),('ACLS_WARTUNGSARBEITEN','http://choicev-cef.net/src/cef/cefFile/variableFile/ACLS_WARTUNGSARBEITEN.jpg',58,80),('ARZT_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/ARZT_ARBEITSVERTRAG.png',125,80),('ARZT_ATTEST','http://choicev-cef.net/src/cef/cefFile/variableFile/ARZT_ATTEST.png',80,44),('ARZT_GUTACHTEN','http://choicev-cef.net/src/cef/cefFile/variableFile/ARZT_GUTACHTEN.png',58,80),('ATTORNEY_ANSCHREIBEN','http://choicev-cef.net/src/cef/cefFile/variableFile/ATTORNEY_ANSCHREIBEN.jpg',56,80),('ATTORNEY_KLAGE','http://choicev-cef.net/src/cef/cefFile/variableFile/ATTORNEY_KLAGE.jpg',58,80),('ATTORNEY_KLAGEZUSATZ','http://choicev-cef.net/src/cef/cefFile/variableFile/ATTORNEY_KLAGEZUSATZ.jpg',58,80),('ATTORNEY_PAPER','http://choicev-cef.net/src/cef/cefFile/variableFile/ATTORNEY_PAPER.jpg',57,80),('COURT_DURCHSUCHUNGSBEFEHL','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_DURCHSUCHUNGSBEFEHL.jpg',58,80),('COURT_HAFTBEFEHL','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_HAFTBEFEHL.jpg',58,80),('COURT_HEARING_1','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_HEARING_1.jpg',58,80),('COURT_HEARING_PLUS','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_HEARING_PLUS.jpg',58,80),('COURT_PAPER','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_PAPER.jpg',53,80),('COURT_URTEIL','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_URTEIL.jpg',58,80),('COURT_VORLADUNG','http://choicev-cef.net/src/cef/cefFile/variableFile/COURT_VORLADUNG.jpg',58,80),('FD_Arbeitsvertrag','http://choicev-cef.net/src/cef/cefFile/variableFile/FD_ARBEITSVERTRAG.png',123.08,80),('FD_FIREPROTECTIONREPORT','http://choicev-cef.net/src/cef/cefFile/variableFile/FD_FIREPROTECTIONREPORT.png',60,80),('FIRSTAIDCERTIFICATE','http://choicev-cef.net/src/cef/cefFile/variableFile/FIRSTAIDCERTIFICATE.jpg',58.6,82.5),('LSSPED_ARBEITSNACHWEIS','http://choicev-cef.net/src/cef/cefFile/variableFile/LSSPED_ARBEITSNACHWEIS.jpg',58,80),('LSSPED_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/LSSPED_ARBEITSVERTRAG.jpg',58,80),('LSSPED_AUFTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/LSSPED_AUFTRAG.jpg',58,80),('MD_BESCHEINIGUNG','http://choicev-cef.net/src/cef/cefFile/variableFile/MD_BESCHEINIGUNG.png',63,90),('MD_KRANKENSCHEIN','http://choicev-cef.net/src/cef/cefFile/variableFile/MD_KRANKENSCHEIN.png',85,60),('MD_TODESBESCHEINIGUNG','http://choicev-cef.net/src/cef/cefFile/variableFile/MD_TODESBESCHEINIGUNG.png',63,90),('PD_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/PD_ARBEITSVERTRAG.png',135,90),('PD_STRAFZETTEL','http://choicev-cef.net/src/cef/cefFile/variableFile/PD_Strafzettel.png',64.1,80),('PDM_KAUFVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/PDM_KAUFVERTRAG.png',58,80),('SAIC_AGB','http://choicev-cef.net/src/cef/cefFile/variableFile/SAIC_AGB.jpg',58,80),('SAIC_GESUNDHEIT','http://choicev-cef.net/src/cef/cefFile/variableFile/SAIC_GESUNDHEIT.png',135,90),('SAIC_KFZ','http://choicev-cef.net/src/cef/cefFile/variableFile/SAIC_KFZ.png',117,80),('SAIC_RECHTSCHUTZ','http://choicev-cef.net/src/cef/cefFile/variableFile/SAIC_RECHTSCHUTZ.png',117,80),('SANI_ARBEITSANWEISUNGEN','http://choicev-cef.net/src/cef/cefFile/variableFile/SANI_ARBEITSANWEISUNG.png',58,80),('SANI_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/SANI_ARBEITSVERTRAG.png',58,80),('SANI_INVOICE','http://choicev-cef.net/src/cef/cefFile/variableFile/SANI_INVOICE.png',58,80),('SANI_ZONE','http://choicev-cef.net/src/cef/cefFile/variableFile/SANI_ZONEN.png',58,90),('SD_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/SD_ARBEITSVERTRAG.png',130,90),('SD_BESCHEINIGUNG','http://choicev-cef.net/src/cef/cefFile/variableFile/SD_BESCHEINIGUNG.png',63,90),('SD_FUEHRUNGSZEUGNISS','http://choicev-cef.net/src/cef/cefFile/variableFile/SD_FUEHRUNGSZEUGNISS.png',63,90),('SD_STRAFZETTEL','http://choicev-cef.net/src/cef/cefFile/variableFile/SD_strafzettel.png',64,80),('SV_ANTRAG_FINANZ_GEWERBE','http://choicev-cef.net/src/cef/cefFile/variableFile/SV_ANTRAG_FINANZ_GEWERBE.png',128,90),('SV_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/SV_ARBEITSVERTRAG.png',135,90),('SV_GEWERBELIZENZ','http://choicev-cef.net/src/cef/cefFile/variableFile/SV_GEWERBELIZENZ.png',116,80),('SV_SOZIALGELD','http://choicev-cef.net/src/cef/cefFile/variableFile/SV_SOZIALGELD.png',120,80),('SV_VORLAUF_LIZENZ','http://choicev-cef.net/src/cef/cefFile/variableFile/SV_VORLAUF_LIZENZ.png',65,90),('TRAFFIC_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/TRAFFIC_ARBEITSVERTRAG.png',173,80),('TRANSP_ARBEITSVERTRAG','http://choicev-cef.net/src/cef/cefFile/variableFile/TRANSP_ARBEITSVERTRAG.png',173,80),('VB_VOLLMACHT_B','http://choicev-cef.net/src/cef/cefFile/variableFile/VB_VOLLMACHT_B.png',58,80),('VB_VOLMACHT_V','http://choicev-cef.net/src/cef/cefFile/variableFile/VB_VOLLMACHT_V.png',58,80);
/*!40000 ALTER TABLE `configvariablefiles` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:09
