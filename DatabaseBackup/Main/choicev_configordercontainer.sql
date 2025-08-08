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
-- Table structure for table `configordercontainer`
--

DROP TABLE IF EXISTS `configordercontainer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configordercontainer` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `category` varchar(45) NOT NULL,
  `position` text NOT NULL,
  `interactCollision` text NOT NULL,
  `unloadCollisions` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configordercontainer`
--

LOCK TABLES `configordercontainer` WRITE;
/*!40000 ALTER TABLE `configordercontainer` DISABLE KEYS */;
INSERT INTO `configordercontainer` VALUES (3,'Essenscontainer','FOOD','{\"X\":71.88132,\"Y\":-2488.391,\"Z\":5.993408}','{\"X\":66.46119,\"Y\":-2493.6826,\"Z\":4.993408}#5.818731#14.141374#54.090942#true#false#true','{\"X\":70.95536,\"Y\":-2487.0618,\"Z\":4.993408}#5.077432#11.825527#236.64575#false#true#false'),(4,'Container 2','NONE','{\"X\":60.54066,\"Y\":-2479.0417,\"Z\":5.993408}','{\"X\":56.307766,\"Y\":-2482.3618,\"Z\":4.993408}#4.3981647#14.960351#56.441986#true#false#true','{\"X\":60.54066,\"Y\":-2479.0417,\"Z\":4.993408}#8.320117#-11.600585#53.121887#false#true#false'),(5,'Kleidungscontainer','CLOTHES','{\"X\":48.224174,\"Y\":-2519.1296,\"Z\":5.993408}','{\"X\":47.27344,\"Y\":-2521.5725,\"Z\":4.993408}#5.818731#14.771222#53.655975#true#false#true','{\"X\":50.224174,\"Y\":-2516.2441,\"Z\":4.993408}#9.885611#5.0#144.50818#false#true#false'),(6,'Sonstigescontainer','MISC','{\"X\":35.459343,\"Y\":-2513.5386,\"Z\":5.993408}','{\"X\":34.459343,\"Y\":-2515.7642,\"Z\":4.993408}#5.0#13.9021635#55.63852#true#false#true','{\"X\":39.910423,\"Y\":-2509.0874,\"Z\":4.993408}#11.676622#5.0#146.88568#false#true#false'),(7,'Container 5','NONE','{\"X\":25.279121,\"Y\":-2505.2834,\"Z\":5.993408}','{\"X\":25.279121,\"Y\":-2505.2834,\"Z\":4.9934083}#5.0#16.127705#55.63852#true#false#true','{\"X\":27.504662,\"Y\":-2500.8323,\"Z\":4.993408}#5.7124333#12.389056#53.948944#false#true#false'),(8,'Werkzeugcontainer','TOOLS','{\"X\":16.826374,\"Y\":-2458.5496,\"Z\":5.993408}','{\"X\":16.826374,\"Y\":-2458.5496,\"Z\":4.993408}#13.9021635#2.7744591#144.56168#true#false#true','{\"X\":21.277456,\"Y\":-2451.873,\"Z\":4.993408}#11.676622#5.0#144.66016#false#true#false'),(9,'Container 7','NONE','{\"X\":6.4615383,\"Y\":-2448.0923,\"Z\":5.993408}','{\"X\":6.4615383,\"Y\":-2448.0923,\"Z\":4.993408}#12.771199#5.0#146.08514#true#false#true','{\"X\":9.781655,\"Y\":-2444.7722,\"Z\":4.993408}#11.640234#5.0#146.08514#false#true#false'),(10,'Container 8','NONE','{\"X\":-4.9582405,\"Y\":-2440.1274,\"Z\":5.993408}','{\"X\":-4.9582405,\"Y\":-2440.1274,\"Z\":4.993408}#12.640234#3.0#145.29083#true#false#true','{\"X\":-1.9582405,\"Y\":-2436.1274,\"Z\":4.993408}#12.0#5.0#144.0#false#true#false'),(11,'Getränkecontainer','DRINKS','{\"X\":-4.246155,\"Y\":-2487.7188,\"Z\":5.993408}','{\"X\":-4.246155,\"Y\":-2487.7188,\"Z\":4.993408}#14.466356#3.1778812#143.94739#true#false#true','{\"X\":1.2202016,\"Y\":-2482.2524,\"Z\":4.993408}#14.110594#5.0#143.94739#false#true#false'),(12,'Container 10','NONE','{\"X\":-14.690109,\"Y\":-2479.6616,\"Z\":5.993408}','{\"X\":-16.512228,\"Y\":-2479.6616,\"Z\":4.993408}#15.932713#5.0#145.0636#true#false#true','{\"X\":-11.045872,\"Y\":-2474.1953,\"Z\":4.993408}#6.8221188#11.273201#53.412994#false#true#false'),(13,'Container 11','NONE','{\"X\":-67.17363,\"Y\":-2421.0461,\"Z\":5.993408}','{\"X\":-67.17363,\"Y\":-2421.0461,\"Z\":4.993408}#13.9021635#2.7744591#0.0#true#false#true','{\"X\":-67.17363,\"Y\":-2427.7227,\"Z\":4.993408}#11.676622#7.225541#0.0#false#true#false'),(14,'Fahrzeugteilecontainer','CAR_PARTS','{\"X\":-80.70329,\"Y\":-2421.8242,\"Z\":5.993408}','{\"X\":-80.70329,\"Y\":-2419.5986,\"Z\":4.993408}#13.9021635#7.225541#0.0#true#false#true','{\"X\":-80.70329,\"Y\":-2426.2754,\"Z\":4.993408}#11.676622#5.0#0.0#false#true#false'),(15,'Container 13','NONE','{\"X\":-95.53846,\"Y\":-2421.3362,\"Z\":5.993408}','{\"X\":-95.53846,\"Y\":-2419.1106,\"Z\":4.993408}#13.9021635#7.225541#0.0#true#false#true','{\"X\":-95.53846,\"Y\":-2425.7874,\"Z\":4.993408}#11.676622#5.0#0.0#false#true#false'),(16,'Chemiecontainer','CHEMIC','{\"X\":-78.44835,\"Y\":-2478.3823,\"Z\":5.993408}','{\"X\":-78.44835,\"Y\":-2478.3823,\"Z\":4.993408}#2.7744591#13.9021635#53.412994#true#false#true','{\"X\":-80.67389,\"Y\":-2482.8335,\"Z\":4.993408}#5.0#11.676622#55.63852#false#true#false'),(17,'Container 15','NONE','{\"X\":-89.775826,\"Y\":-2470.6682,\"Z\":5.993408}','{\"X\":-89.775826,\"Y\":-2470.6682,\"Z\":4.993408}#13.9021635#2.7744591#144.66016#true#false#true','{\"X\":-92.001366,\"Y\":-2475.1194,\"Z\":4.993408}#5.0#-12.804327#53.412994#false#true#false'),(18,'Möbelcontainer','INTERIOR','{\"X\":-90.8967,\"Y\":-2496.3296,\"Z\":5.993408}','{\"X\":-90.8967,\"Y\":-2496.3296,\"Z\":4.993408}#2.7744591#13.9021635#53.412994#true#false#true','{\"X\":-93.12224,\"Y\":-2500.7808,\"Z\":4.993408}#13.9021635#5.0#144.66016#false#true#false'),(19,'Container 17','NONE','{\"X\":-104.61099,\"Y\":-2489.6309,\"Z\":5.993408}','{\"X\":-102.38545,\"Y\":-2487.4053,\"Z\":4.993408}#13.9021635#5.0#144.66016#true#false#true','{\"X\":-106.83653,\"Y\":-2491.8564,\"Z\":4.993408}#11.676622#5.0#144.66016#false#true#false'),(20,'Fahrzeugcontainer','CAR','{\"X\":-95.90769,\"Y\":-2415.5605,\"Z\":5.993408}','{\"X\":-94.90769,\"Y\":-2415.5605,\"Z\":4.993408}#15.0#17.0#0.0#true#false#true','{\"X\":-109.00769,\"Y\":-2410.1606,\"Z\":4.993408}#12.1#3.0#0.0#false#true#false*{\"X\":-108.66154,\"Y\":-2412.9539,\"Z\":4.993408}#12.2#3.0#0.0#false#true#false'),(21,'Sicherheitscontainer','SECURED','{\"X\":-88.71868,\"Y\":-2545.554,\"Z\":5.993408}','{\"X\":-84.15832,\"Y\":-2546.6438,\"Z\":5.010254}#11.611956#5.991793#56.96991#true#false#true','{\"X\":-92.11682,\"Y\":-2543.8308,\"Z\":4.993408}#4.3233237#10.413411#54.6127#false#true#false'),(22,'Waffencontainer','WEAPONS','{\"X\":-91.99121,\"Y\":-2558.2021,\"Z\":5.993408}','{\"X\":-90.98075,\"Y\":-2557.709,\"Z\":4.993408}#10.67173#3.5204182#54.04886#true#false#true','{\"X\":-97.5464,\"Y\":-2552.453,\"Z\":4.993408}#4.2121453#10.047305#54.075012#false#true#false');
/*!40000 ALTER TABLE `configordercontainer` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:16
