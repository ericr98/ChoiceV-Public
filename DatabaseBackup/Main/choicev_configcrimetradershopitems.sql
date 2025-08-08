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
-- Table structure for table `configcrimetradershopitems`
--

DROP TABLE IF EXISTS `configcrimetradershopitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configcrimetradershopitems` (
  `id` int NOT NULL AUTO_INCREMENT,
  `configItemId` int NOT NULL,
  `shop` varchar(45) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `maxPerRequest` int NOT NULL,
  `reputationRequirement` int NOT NULL,
  `sellNotBuy` tinyint(1) NOT NULL,
  `priceIsFavors` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configcrimetradershop_configItemid_idx` (`configItemId`),
  CONSTRAINT `fk_configcrimetradershop_configItemid` FOREIGN KEY (`configItemId`) REFERENCES `configitems` (`configItemId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configcrimetradershopitems`
--

LOCK TABLES `configcrimetradershopitems` WRITE;
/*!40000 ALTER TABLE `configcrimetradershopitems` DISABLE KEYS */;
INSERT INTO `configcrimetradershopitems` VALUES (3,618,'DEALER_DRUGS',15.00,10,0,0,0),(4,619,'DEALER_DRUGS',21.00,10,0,0,0),(5,620,'DEALER_DRUGS',10.00,10,0,0,0),(6,621,'DEALER_DRUGS',27.00,10,0,0,0),(7,622,'FENCE_STUFF',20.00,10,0,1,0),(8,623,'FENCE_STUFF',30.00,10,0,1,0),(9,624,'FENCE_STUFF',18.00,10,0,1,0),(10,625,'FENCE_STUFF',35.00,10,0,1,0),(11,626,'FENCE_STUFF',27.00,10,0,1,0);
/*!40000 ALTER TABLE `configcrimetradershopitems` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:49
