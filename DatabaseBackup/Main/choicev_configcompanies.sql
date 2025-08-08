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
-- Table structure for table `configcompanies`
--

DROP TABLE IF EXISTS `configcompanies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configcompanies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `shortName` varchar(20) NOT NULL,
  `companyCity` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `companyStreet` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `companyTax` float DEFAULT NULL,
  `companyType` int NOT NULL DEFAULT '-1',
  `riskLevel` int NOT NULL,
  `position` text CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `blipType` int DEFAULT NULL,
  `blipColor` int DEFAULT NULL,
  `maxEmployees` int NOT NULL,
  `reputation` int NOT NULL,
  `companyBankAccount` bigint DEFAULT NULL,
  `invoiceId` int DEFAULT '1',
  `buildingType` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `companies_id_uindex` (`id`),
  KEY `fk_companies_companybankaccount_idx` (`companyBankAccount`),
  CONSTRAINT `fk_companies_companybankaccount` FOREIGN KEY (`companyBankAccount`) REFERENCES `bankaccounts` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configcompanies`
--

LOCK TABLES `configcompanies` WRITE;
/*!40000 ALTER TABLE `configcompanies` DISABLE KEYS */;
INSERT INTO `configcompanies` VALUES (1,'Los Santos Sheriff Department','SD',NULL,NULL,NULL,9,0,'{\"X\":-440.72968,\"Y\":6003.4814,\"Z\":31.706177}',NULL,NULL,1,0,858688670,7,0),(2,'Los Santos Police Department','PD',NULL,NULL,NULL,1,0,'{\"X\":831.53406,\"Y\":-1290.0923,\"Z\":28.235107}',NULL,NULL,1,0,655811902,9,0),(3,'Los Santos Fire Department','FD',NULL,NULL,NULL,3,0,'{\"X\":1203.2175,\"Y\":-3254.9275,\"Z\":7.088623}',NULL,NULL,1,0,655247017,1,0),(4,'Los Santos Medical Department','MD',NULL,NULL,NULL,2,0,'{\"X\":1199.2087,\"Y\":-3257.2087,\"Z\":7.088623}',NULL,NULL,1,0,655780472,14,0),(5,'Department of Justice','JAS',NULL,NULL,NULL,-1,0,'{\"X\":-444.03955,\"Y\":6005.222,\"Z\":31.706177}',NULL,NULL,1,0,858885532,2,0),(10,'Daily Globe','DG',NULL,NULL,NULL,-1,0,'{\"X\":123.91648,\"Y\":-756.54065,\"Z\":186.10107}',NULL,NULL,1,0,655436598,1,0),(12,'Stadtverwaltung','LOS',NULL,NULL,NULL,11,0,'{\"X\":123.50769,\"Y\":-755.4066,\"Z\":186.10107}',NULL,NULL,1,0,858939976,56,0),(13,'Automobilclub Los Santos','ACLS',NULL,NULL,NULL,16,0,'{\"X\":123.7055,\"Y\":-756.422,\"Z\":186.10107}',NULL,NULL,1,0,655679619,150,0),(14,'Dr.Goodmann','DGM',NULL,NULL,NULL,2,0,'{\"X\":123.69231,\"Y\":-756.4088,\"Z\":186.10107}',NULL,NULL,1,0,775583146,11,0),(17,'Los Santos Spedition','SPED',NULL,NULL,NULL,-1,0,'{\"X\":123.69231,\"Y\":-756.4088,\"Z\":186.10107}',NULL,NULL,1,0,655740135,80,0),(18,'Los Santos Department of Sanitation','LSDS',NULL,NULL,NULL,-1,0,'{\"X\":124.00879,\"Y\":-755.4198,\"Z\":186.10107}',NULL,NULL,1,0,858138184,1,0),(19,'Trafic School and Transportation Service','TSATS',NULL,NULL,NULL,-1,0,'{\"X\":124.00879,\"Y\":-755.4198,\"Z\":186.10107}',NULL,NULL,1,0,858377122,13,0),(20,'City Bike Shop','CBAS',NULL,NULL,NULL,15,0,'{\"X\":124.00879,\"Y\":-755.4198,\"Z\":186.10107}',NULL,NULL,1,0,655385304,7,0),(21,'Magicmoon','MGM',NULL,NULL,NULL,-1,0,'{\"X\":123.9033,\"Y\":-755.55164,\"Z\":186.1853}',NULL,NULL,1,0,655958257,1,0),(22,'Baubetrieb Skiver','BBS',NULL,NULL,NULL,-1,0,'{\"X\":123.9033,\"Y\":-755.55164,\"Z\":186.1853}',NULL,NULL,1,0,775617509,8,0),(24,'Premium Deluxe Motorsport','PDM',NULL,NULL,NULL,15,0,'{\"X\":123.9033,\"Y\":-755.55164,\"Z\":186.1853}',NULL,NULL,1,0,858747202,82,0),(25,'Hookies','HKS',NULL,NULL,NULL,-1,0,'{\"X\":123.890114,\"Y\":-755.5648,\"Z\":186.1853}',NULL,NULL,1,0,775788233,93,0),(28,'San Andreas Insurance Company','SAIC',NULL,NULL,NULL,-1,0,'{\"X\":-540.8044,\"Y\":-586.0747,\"Z\":34.671753}',NULL,NULL,1,0,858622538,11,0),(30,'Anwaltskanzlei Vega & Blair','AVB',NULL,NULL,NULL,-1,0,'{\"X\":-1463.4857,\"Y\":-542.2286,\"Z\":79.2395}',NULL,NULL,1,0,655956512,5,0),(33,'Kurant Bar','KRNT',NULL,NULL,NULL,-1,0,'{\"X\":376.48352,\"Y\":-1081.3055,\"Z\":29.46521}',NULL,NULL,1,0,775932994,3,0),(34,'Hill Valley Community Church','HVCC',NULL,NULL,NULL,-1,0,'{\"X\":-1680.6329,\"Y\":-281.26154,\"Z\":51.858643}',NULL,NULL,1,0,775301717,1,0),(35,'Style & Performance Society','SaPS',NULL,NULL,NULL,16,0,'{\"X\":128.72968,\"Y\":-211.68791,\"Z\":54.50403}',NULL,NULL,1,0,655459599,2,0),(36,'Mediterraneo Italian Cuccina','MIC',NULL,NULL,NULL,-1,0,'{\"X\":422.95386,\"Y\":-1501.1736,\"Z\":30.13916}',NULL,NULL,1,0,655301480,23,0),(37,'Mediterraneo','MEDIT',NULL,NULL,NULL,-1,0,'{\"X\":417.07254,\"Y\":-1492.6022,\"Z\":30.13916}',NULL,NULL,1,0,775707711,1,0);
/*!40000 ALTER TABLE `configcompanies` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:48
