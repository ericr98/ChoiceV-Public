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
-- Table structure for table `configitemsfoodcategories`
--

DROP TABLE IF EXISTS `configitemsfoodcategories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configitemsfoodcategories` (
  `category` varchar(45) NOT NULL,
  `hunger` float NOT NULL,
  `thirst` float NOT NULL,
  `energy` float NOT NULL,
  PRIMARY KEY (`category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configitemsfoodcategories`
--

LOCK TABLES `configitemsfoodcategories` WRITE;
/*!40000 ALTER TABLE `configitemsfoodcategories` DISABLE KEYS */;
INSERT INTO `configitemsfoodcategories` VALUES ('ALCOHOL',0,40,-15),('ALCOHOL_BIG',0,100,-30),('CIGARETTE',0,0,5),('ENERGY_DRINK',0,30,60),('FRUITS',20,15,35),('HOT_BEVERAGE',0,55,40),('RESTAURANT_ALCOHOL',0,60,-15),('RESTAURANT_MAIN',200,0,200),('RESTAURANT_SNACKS',130,0,130),('RESTAURANT_SOFTDRINK',0,120,0),('SNACK_HEALTHY',25,0,45),('SNACK_UNHEALTHY',25,0,30),('SOFT_DRINK',0,50,20),('STORE_FAST_FOOD',100,0,45),('UNDRINKABLE',-15,-15,-15),('UNEATABLE',-15,-15,-15),('VEGETABLES',20,10,50),('WATER',0,100,0);
/*!40000 ALTER TABLE `configitemsfoodcategories` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:27
