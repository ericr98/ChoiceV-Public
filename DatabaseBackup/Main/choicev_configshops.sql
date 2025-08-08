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
-- Table structure for table `configshops`
--

DROP TABLE IF EXISTS `configshops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configshops` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `position` text NOT NULL,
  `width` float NOT NULL,
  `height` float NOT NULL,
  `rotation` float NOT NULL,
  `inventoryId` int DEFAULT NULL,
  `type` int NOT NULL,
  `robValue` decimal(10,2) NOT NULL DEFAULT '0.00',
  `maxRovValue` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=70 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configshops`
--

LOCK TABLES `configshops` WRITE;
/*!40000 ALTER TABLE `configshops` DISABLE KEYS */;
INSERT INTO `configshops` VALUES (1,'Snackautomat','{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}',0,0,0,NULL,1,0.00,0.00),(2,'Getränkeautomat','{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}',0,0,0,NULL,2,0.00,0.00),(3,'Wasserautomat','{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}',0,0,0,NULL,3,0.00,0.00),(4,'Kaffeeautomat','{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}',0,0,0,NULL,4,0.00,0.00),(5,'Zigarettenautomat','{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}',0,0,0,NULL,9,0.00,0.00),(15,'Grove Tankstelle','{\"X\":-50.3229,\"Y\":-1753.4108,\"Z\":29.414673}',7.63082,14.0613,49.3872,NULL,6,120.00,120.00),(16,'Mirror Park Tankstelle','{\"X\":1159.0544,\"Y\":-322.68155,\"Z\":69.19702}',13.6565,7.64478,9.06147,NULL,6,120.00,120.00),(17,'Little Seoul Tankstelle','{\"X\":-711.8359,\"Y\":-912.533,\"Z\":19.203613}',13.5261,7.70671,0,NULL,6,120.00,120.00),(18,'Vespucci Liquor','{\"X\":-1223.9382,\"Y\":-906.55664,\"Z\":12.312134}',6.89469,7.3007,34.6458,NULL,7,120.00,120.00),(19,'Morningwood Liquor','{\"X\":-1488.0637,\"Y\":-379.72968,\"Z\":40.14795}',8,8,43.6011,NULL,7,120.00,120.00),(20,'Murrieta Heights Liquor','{\"X\":1136.924,\"Y\":-981.58057,\"Z\":46.399292}',7.3007,7.57137,7.17277,NULL,7,120.00,120.00),(21,'24/7 Vinewood Mitte','{\"X\":377.3872,\"Y\":327.2575,\"Z\":103.55383}',7.53002,11.7471,75.5479,NULL,5,120.00,120.00),(22,'24/7 Strawberry','{\"X\":29.116486,\"Y\":-1345.5284,\"Z\":29.482056}',11.4115,7.21937,0,NULL,5,120.00,120.00),(23,'24/7 Harmony','{\"X\":544.72614,\"Y\":2668.9412,\"Z\":42.153076}',11.1649,7.21937,6.90472,NULL,5,120.00,120.00),(24,'Harmony Liquor','{\"X\":1166.3116,\"Y\":2707.9568,\"Z\":38.142822}',7.71257,8.20576,0,NULL,7,120.00,120.00),(25,'24/7 East-Highway','{\"X\":2678.7957,\"Y\":3284.428,\"Z\":55.228516}',11.0213,7.79811,60.5365,NULL,5,120.00,120.00),(27,'24/7 Sandy Shores','{\"X\":1963.2687,\"Y\":3743.9473,\"Z\":32.329712}',11.2865,7.37903,29.5728,NULL,5,120.00,120.00),(29,'Grapeseed Tanke','{\"X\":1702.1143,\"Y\":4926.5513,\"Z\":42.052002}',14.3832,7.98529,53.1227,NULL,6,120.00,120.00),(30,'Sandy Shores Liquor','{\"X\":1394.3241,\"Y\":3603.9238,\"Z\":34.975098}',12.2683,7.42276,19.7859,NULL,7,120.00,120.00),(31,'24/7 Paleto Highway','{\"X\":1733.33,\"Y\":6414.4863,\"Z\":35.025635}',7.37668,12.4929,63.583,NULL,5,120.00,120.00),(32,'24/7 West-Highway','{\"X\":-3042.212,\"Y\":588.56604,\"Z\":7.897461}',7.24664,11.7399,17.7485,NULL,5,120.00,120.00),(33,'West Highway Liquor','{\"X\":-2968.9338,\"Y\":390.45255,\"Z\":15.041748}',8.31314,9.36911,86.6856,NULL,7,120.00,120.00),(34,'Ersatz-Baumarkt','{\"X\":2749.1472,\"Y\":3466.5598,\"Z\":55.717163}',2.6993,2.6993,0,NULL,10,80.00,80.00),(35,'Burgerstand Del Perro','{\"X\":-1856.2273,\"Y\":-1223.4066,\"Z\":13.00293}',4.63212,3.12248,45.8678,NULL,13,12.00,12.00),(36,'Burgerstand Terminal','{\"X\":822.5712,\"Y\":-2976.5408,\"Z\":6.010254}',3.78198,3.37598,0,NULL,13,12.00,12.00),(37,'Simmet Alley Imbiss','{\"X\":463.44846,\"Y\":-700.7466,\"Z\":27.51062}',3.78198,2.83464,0,NULL,11,12.00,12.00),(38,'Richman Glen Tanke','{\"X\":-1824.9758,\"Y\":791.3849,\"Z\":138.18018}',13.9912,7.81008,42.9052,NULL,6,120.00,120.00),(39,'24/7 East-East-Highway','{\"X\":2555.6492,\"Y\":385.54898,\"Z\":108.608765}',7.22316,11.4467,0,NULL,5,120.00,120.00),(40,'MD Kantine','{\"X\":-332.02765,\"Y\":-236.79573,\"Z\":34.385376}',4.45119,4.75244,53.3614,NULL,14,170.00,170.00),(41,'Randy McKois Outdoor','{\"X\":-771.5724,\"Y\":5601.781,\"Z\":33.744995}',5.95737,11.3158,76.9616,NULL,15,85.00,85.00),(42,'Hotdogs Sandy Shores','{\"X\":1983.4557,\"Y\":3707.0881,\"Z\":32.076904}',2.96997,5.67668,0,NULL,12,8.00,8.00),(43,'Obststand Vinewood Bowl','{\"X\":1045.1259,\"Y\":696.9729,\"Z\":158.83813}',6.79732,5.44933,58.7175,NULL,8,12.00,12.00),(44,'Obststand Vinewood Hills','{\"X\":-1380.5548,\"Y\":735.14874,\"Z\":182.96704}',6.34799,5.44933,0,NULL,8,12.00,12.00),(45,'Obststand Windpark','{\"X\":2528.0186,\"Y\":2038.153,\"Z\":19.827148}',5.2019,5.60569,0,NULL,8,12.00,12.00),(46,'Obststand Vinewood Hills/ Wüste','{\"X\":149.02147,\"Y\":1669.25,\"Z\":228.86597}',5.73576,5.73576,78.3936,NULL,8,12.00,12.00),(47,'Obststand Paleto Bay Tanke','{\"X\":164.87784,\"Y\":6633.344,\"Z\":31.621948}',5,5,52.1024,NULL,8,12.00,12.00),(48,'Obststand Alamo Fruit Market','{\"X\":1792.4521,\"Y\":4592.047,\"Z\":37.67102}',7.19525,6.64643,0,NULL,8,12.00,12.00),(49,'Obststand Route 68','{\"X\":-458.9438,\"Y\":2862.046,\"Z\":35.261597}',6.64643,6.09762,15.9155,NULL,8,12.00,12.00),(50,'Obststand West-Highway','{\"X\":-3025.5579,\"Y\":369.10703,\"Z\":14.637329}',7.56914,5.54881,74.3206,NULL,8,12.00,12.00),(51,'Hotdogs Del Perro','{\"X\":-1637.2603,\"Y\":-1083.931,\"Z\":13.053467}',3.52848,4.63212,7.72547,NULL,12,8.00,8.00),(52,'Burger Del Perro Pier','{\"X\":-1691.8854,\"Y\":-1134.8348,\"Z\":13.137695}',4.63212,4.26424,0,NULL,13,8.00,8.00),(53,'Burger Vespucci','{\"X\":-1233.3613,\"Y\":-1486.0656,\"Z\":4.359009}',5,5,0,NULL,13,8.00,8.00),(54,'Hotdogs Vespucci','{\"X\":-1220.9025,\"Y\":-1505.0282,\"Z\":4.342163}',5,5,0,NULL,12,8.00,8.00),(55,'24/7 Chumash (West-Highway)','{\"X\":-3243.7676,\"Y\":1004.7445,\"Z\":12.817627}',11.3334,7.21937,86.0103,NULL,5,120.00,120.00),(58,'24/7 Paleto City','{\"X\":-15.932294,\"Y\":6487.571,\"Z\":31.520874}',14.3707,9.68534,44.4826,NULL,5,120.00,120.00),(59,'Ammuniation Downtown','{\"X\":20.422556,\"Y\":-1109.5912,\"Z\":29.7854}',12.1346,7.74406,67.3618,NULL,17,55.00,55.00),(60,'Ammunation Sandy Shores','{\"X\":1694.5187,\"Y\":3757.4768,\"Z\":34.6886}',8.66421,13.5498,45.4758,NULL,17,45.00,45.00),(61,'Ammunation Route 68','{\"X\":-1116.9626,\"Y\":2696.2417,\"Z\":18.546509}',7.44281,13.5498,42.7491,NULL,17,30.00,30.00),(62,'Ammunation Paleto Bay','{\"X\":-328.5799,\"Y\":6080.971,\"Z\":31.453491}',7.81069,11.6072,43.9705,NULL,17,30.00,30.00),(63,'Ammunation Chumash','{\"X\":-3168.994,\"Y\":1086.0,\"Z\":20.838135}',7.44281,11.107,67.1772,NULL,17,30.00,30.00),(64,'Ammunation Tataviam','{\"X\":2568.463,\"Y\":297.29398,\"Z\":108.726685}',7.94304,11.6218,0,NULL,17,0.00,0.00),(65,'Ammunation Del Perro','{\"X\":-1307.8418,\"Y\":-392.9011,\"Z\":36.693726}',7.44281,12.3284,78.1698,NULL,17,0.00,0.00),(66,'Ammunation Cypress Flats','{\"X\":810.8571,\"Y\":-2154.3203,\"Z\":29.616821}',6.8394,11.6218,0,NULL,17,0.00,0.00),(67,'Ammunation Hawick','{\"X\":249.65161,\"Y\":-48.448353,\"Z\":69.93848}',8.27492,11.5498,70.2589,NULL,17,0.00,0.00),(68,'Ammunation La Mesa','{\"X\":842.9407,\"Y\":-1030.6143,\"Z\":28.18457}',8.27492,11.5498,0,NULL,17,0.00,0.00),(69,'Ammunation Little Seoul','{\"X\":-663.0198,\"Y\":-938.4011,\"Z\":21.81543}',8.27492,11.5498,0,NULL,17,0.00,0.00);
/*!40000 ALTER TABLE `configshops` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:12
