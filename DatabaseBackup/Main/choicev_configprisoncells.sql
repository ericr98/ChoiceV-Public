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
-- Table structure for table `configprisoncells`
--

DROP TABLE IF EXISTS `configprisoncells`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configprisoncells` (
  `id` int NOT NULL AUTO_INCREMENT,
  `prisonId` int NOT NULL,
  `inmateId` int DEFAULT NULL,
  `name` varchar(45) NOT NULL,
  `collisionShape` text NOT NULL,
  `inventoryId` int DEFAULT NULL,
  `combination` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `prisoncells___fk_inventoryId` (`inventoryId`),
  KEY `prisoncells___fk_prisonId` (`prisonId`),
  KEY `fk_configprisoncells_inmateId_idx` (`inmateId`),
  CONSTRAINT `fk_configprisoncells_inmateId` FOREIGN KEY (`inmateId`) REFERENCES `prisoninmates` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `prisoncells___fk_inventoryId` FOREIGN KEY (`inventoryId`) REFERENCES `inventories` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `prisoncells___fk_prisonId` FOREIGN KEY (`prisonId`) REFERENCES `configprisons` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configprisoncells`
--

LOCK TABLES `configprisoncells` WRITE;
/*!40000 ALTER TABLE `configprisoncells` DISABLE KEYS */;
INSERT INTO `configprisoncells` VALUES (1,1,NULL,'Zelle 01','{\"X\":-1024.6022,\"Y\":4904.9404,\"Z\":172.48645}#1.4053683#1.4053683#0.0#true#false#true',31940,'[1,2,3,4]'),(2,1,NULL,'Zelle 02','{\"X\":-1032.2357,\"Y\":4895.523,\"Z\":172.48645}#0.8887718#1.2436079#49.816772#true#false#true',31939,'[3,3,6]'),(3,1,NULL,'Zelle 03','{\"X\":-1022.8615,\"Y\":4903.385,\"Z\":172.48645}#1.0673425#1.3357917#48.85611#true#false#true',31938,'[4,3,2,1]'),(4,1,NULL,'Zelle 04','{\"X\":-1030.4415,\"Y\":4894.1167,\"Z\":172.48645}#0.8937455#1.1583185#50.60666#true#false#true',31937,'[6,1,8]'),(5,1,NULL,'Zelle 05','{\"X\":-1021.46375,\"Y\":4902.4614,\"Z\":172.48645}#0.7641171#1.0947149#50.722046#true#false#true',31936,'[1,2,3,4]'),(6,1,NULL,'Zelle 07','{\"X\":-1019.7231,\"Y\":4901.077,\"Z\":172.48645}#0.81157374#1.1127679#50.722046#true#false#true',31935,'[5,4,4]'),(7,1,NULL,'Zelle 09','{\"X\":-1018.37805,\"Y\":4899.943,\"Z\":172.4696}#0.9400922#1.1866891#49.801758#true#false#true',31934,'[1,3,4]'),(8,1,NULL,'Zelle 11','{\"X\":-1016.5714,\"Y\":4898.4526,\"Z\":172.4696}#0.783281#1.0844753#49.801758#true#false#true',31933,'[7,4,7]'),(9,1,NULL,'Zelle 13','{\"X\":-1015.2132,\"Y\":4897.477,\"Z\":172.4696}#0.8501124#1.1513065#48.543518#true#false#true',31932,'[4,7,7]'),(10,1,NULL,'Zelle 15','{\"X\":-1013.47253,\"Y\":4896.013,\"Z\":172.4696}#0.8813276#0.99213076#51.18744#true#false#true',31931,'[7,3,8]'),(11,1,NULL,'Zelle 17','{\"X\":-1012.21,\"Y\":4894.6875,\"Z\":172.4696}#1.1350693#1.3212056#47.697784#true#false#true',31930,'[7,2,2]'),(12,1,NULL,'Zelle 19','{\"X\":-1010.3736,\"Y\":4893.508,\"Z\":172.4696}#0.954924#1.2255945#47.082184#true#false#true',31929,'[2,6,6]'),(13,1,NULL,'Zelle 21','{\"X\":-1009.0154,\"Y\":4892.3867,\"Z\":172.4696}#1.1948459#1.1948459#46.246582#true#false#true',31928,'[6,8,8]'),(14,1,NULL,'Zelle 23','{\"X\":-1007.1956,\"Y\":4890.923,\"Z\":172.4696}#1.2459942#1.2459942#54.024963#true#false#true',31927,'[2,8,7]'),(15,1,NULL,'Zelle 25','{\"X\":-1006.15247,\"Y\":4889.868,\"Z\":172.4696}#1.0844753#1.5353266#49.942566#true#false#true',31926,'[0,8,6]'),(16,1,NULL,'Zelle 27','{\"X\":-1004.12305,\"Y\":4888.47,\"Z\":172.4696}#0.78122514#1.2305541#53.121887#true#false#true',31925,'[5,5,8]'),(17,1,NULL,'Zelle 29','{\"X\":-1002.76483,\"Y\":4887.4287,\"Z\":172.4696}#1.2192382#1.2192382#48.961914#true#false#true',31924,'[1,1,2]'),(18,1,NULL,'Zelle 31','{\"X\":-1001.0453,\"Y\":4885.965,\"Z\":172.4696}#0.7461322#1.3515316#50.228455#true#false#true',31923,'[1,2,1]'),(19,1,NULL,'Zelle 33','{\"X\":-999.66595,\"Y\":4884.7646,\"Z\":172.4696}#1.0059502#1.5599661#50.99243#true#false#true',31922,'[5,8,4]'),(20,1,NULL,'Zelle 06','{\"X\":-1029.1384,\"Y\":4893.02,\"Z\":172.48645}#1.0#1.0#49.988922#true#false#true',31921,'[8,7,8]'),(21,1,NULL,'Zelle 08','{\"X\":-1027.4769,\"Y\":4891.7183,\"Z\":172.48645}#-1.0496475#-1.251544#48.397186#true#false#true',31920,'[6,6,5]'),(22,1,NULL,'Zelle 10','{\"X\":-1026.1055,\"Y\":4890.4746,\"Z\":172.48645}#0.96206963#1.3658627#50.342346#true#false#true',31919,'[0,5,3]'),(23,1,NULL,'Zelle 11','{\"X\":-1024.199,\"Y\":4889.112,\"Z\":172.48645}#0.9399415#1.3459474#52.405457#true#false#true',31918,'[0,4,1]'),(25,1,NULL,'Zelle 12','{\"X\":-1024.2461,\"Y\":4889.051,\"Z\":172.48645}#0.79345816#1.1583185#51.209625#true#false#true',31917,'[1,6,7]'),(26,1,NULL,'Zelle 14','{\"X\":-1022.9407,\"Y\":4887.956,\"Z\":172.48645}#-0.6866529#-0.9853753#49.333923#true#false#true',31916,'[2,7,4]'),(27,1,NULL,'Zelle 16','{\"X\":-1021.17365,\"Y\":4886.5317,\"Z\":172.48645}#0.8205867#1.3212056#51.019318#true#false#true',31915,'[2,5,7]'),(28,1,NULL,'Zelle 18','{\"X\":-1019.88135,\"Y\":4885.53,\"Z\":172.48645}#0.83989507#1.3357917#50.747375#true#false#true',31914,'[1,8,0]'),(29,1,NULL,'Zelle 20','{\"X\":-1017.82635,\"Y\":4883.9736,\"Z\":172.4696}#0.90634626#1.2385964#47.48639#true#false#true',31913,'[2,6,4]'),(30,1,NULL,'Zelle 22','{\"X\":-1016.66376,\"Y\":4882.9316,\"Z\":172.4696}#0.9033974#1.1639662#50.499542#true#false#true',31912,'[2,6,0]'),(31,1,NULL,'Zelle 24','{\"X\":-1014.94946,\"Y\":4881.455,\"Z\":172.4696}#0.7508147#1.1546077#51.18744#true#false#true',31911,'[1,4,2]'),(32,1,NULL,'Zelle 26','{\"X\":-1013.5912,\"Y\":4880.479,\"Z\":172.4696}#0.7597036#1.6798831#49.801758#true#false#true',31910,'[1,4,3]'),(33,1,NULL,'Zelle 25','{\"X\":-1011.7714,\"Y\":4878.9624,\"Z\":172.4696}#0.9104357#2.0569644#50.310944#true#false#true',31909,'[2,1,8]'),(34,1,NULL,'Zelle 28','{\"X\":-1011.7846,\"Y\":4879.029,\"Z\":172.4696}#0.811953#0.96791226#52.016113#true#false#true',31908,'[3,7,5]'),(35,1,NULL,'Zelle 30','{\"X\":-1010.46594,\"Y\":4877.9604,\"Z\":172.4696}#1.005847#0.8607883#140.50687#true#false#true',31907,'[4,8,1]'),(36,1,NULL,'Zelle 32','{\"X\":-1008.6857,\"Y\":4876.51,\"Z\":172.4696}#0.9361084#1.3434957#50.717255#true#false#true',31906,'[0,8,6]'),(37,1,NULL,'Zelle 34','{\"X\":-1007.2747,\"Y\":4875.4683,\"Z\":172.4696}#0.919733#1.3357917#49.811523#true#false#true',31905,'[6,2,3]');
/*!40000 ALTER TABLE `configprisoncells` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:05
