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
-- Table structure for table `configipls`
--

DROP TABLE IF EXISTS `configipls`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configipls` (
  `id` int NOT NULL AUTO_INCREMENT,
  `identifier` varchar(45) NOT NULL,
  `iplType` varchar(45) NOT NULL,
  `displayName` varchar(45) NOT NULL,
  `gtaName` varchar(100) NOT NULL,
  `position` text NOT NULL,
  `isLoaded` int NOT NULL,
  `standardLoadedIn` int NOT NULL DEFAULT '0',
  `blipType` int DEFAULT NULL,
  `blipColor` int DEFAULT NULL,
  `island` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=79 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configipls`
--

LOCK TABLES `configipls` WRITE;
/*!40000 ALTER TABLE `configipls` DISABLE KEYS */;
INSERT INTO `configipls` VALUES (1,'CARGO_SHIP','YMAP','Frachtschiff','cargoship','{\"X\":-163,\"Y\":-2385, \"Z\":60}',0,1,410,20,0),(2,'FIB_LOBBY','YMAP','Fib-Lobby','FIBlobby','{\"X\":110,\"Y\":-744, \"Z\":45}',1,1,-1,-1,0),(3,'DOOR_IN_CYPRESS','YMAP','Cypress-Workshop-Türen','bkr_bi_id1_23_door','{\"X\":972,\"Y\":-1849, \"Z\":36}',1,1,-1,-1,0),(4,'Farm','YMAP','ONeils Farm Normal','farm','{\"X\":2469,\"Y\":4955, \"Z\":45}',1,1,-1,-1,0),(5,'Farm interior','YMAP','ONeils Farm Normal','farmint','{\"X\":2469,\"Y\":4955, \"Z\":45}',1,1,-1,-1,0),(6,'Farm','YMAP','ONeils Farm Normal','farm_lod','{\"X\":2469,\"Y\":4955, \"Z\":45}',1,1,-1,-1,0),(7,'Farm','YMAP','ONeils Farm Normal','farm_props','{\"X\":2469,\"Y\":4955, \"Z\":45}',1,1,-1,-1,0),(8,'Farm','YMAP','ONeils Farm Normal','des_farmhouse','{\"X\":2469,\"Y\":4955, \"Z\":45}',1,1,-1,-1,0),(9,'LifeInvader','YMAP','LifeInvader','facelobby','{\"X\":-1047,\"Y\":-233, \"Z\":39}',1,1,-1,-1,0),(10,'LifeInvader','YMAP','LifeInvader','facelobby_lod','{\"X\":-1047,\"Y\":-233, \"Z\":39}',1,1,-1,-1,0),(11,'Train Bridge (No Crash)','YMAP','Train Bridge (No Crash)','canyonriver01','{\"X\":-421.74725,\"Y\":4426.998, \"Z\":27.54312}',1,1,-1,-1,0),(12,'Train Bridge (No Crash)','YMAP','Train Bridge (No Crash)','canyonriver01_lod','{\"X\":532,\"Y\":4526, \"Z\":89}',0,0,-1,-1,0),(13,'Train Bridge (No Crash)','YMAP','Train Bridge (No Crash)','CanyonRvrShallow','{\"X\":-1648.8528,\"Y\":4451.6740, \"Z\":1.3934326}',1,1,-1,-1,0),(14,'Lesters Factory','YMAP','Lesters Factory','id2_14_during1','{\"X\":716,\"Y\":-962, \"Z\":31}',1,1,-1,-1,0),(15,'Lesters Factory','YMAP','Lesters Factory','id2_14_during_door','{\"X\":716,\"Y\":-962, \"Z\":31}',1,1,-1,-1,0),(16,'Clucking Bell Farms','YMAP','Clucking Bell Farms','CS1_02_cf_onmission1','{\"X\":-83,\"Y\":6237, \"Z\":50}',1,1,-1,-1,0),(17,'Clucking Bell Farms','YMAP','Clucking Bell Farms','CS1_02_cf_onmission2','{\"X\":-83,\"Y\":6237, \"Z\":50}',1,1,-1,-1,0),(18,'Clucking Bell Farms','YMAP','Clucking Bell Farms','CS1_02_cf_onmission3','{\"X\":-83,\"Y\":6237, \"Z\":50}',1,1,-1,-1,0),(19,'Clucking Bell Farms','YMAP','Clucking Bell Farms','CS1_02_cf_onmission4','{\"X\":-83,\"Y\":6237, \"Z\":50}',1,1,-1,-1,0),(20,'PDM (Simeons)','INTERIOR','PDM (Simeons)','shr_int','{\"X\":-47.696705,\"Y\":-1097.3671, \"Z\":26.415405}',1,1,-1,-1,0),(21,'Jewel Store','YMAP','Jewel Store','post_hiest_unload','{\"X\":-630,\"Y\":-236, \"Z\":38}',1,1,-1,-1,0),(22,'Max Renda Shop','YMAP','Max Renda Shop','refit_unload','{\"X\":-585,\"Y\":-282, \"Z\":35}',1,1,-1,-1,0),(23,'Union Depository','YMAP','Union Depository','FINBANK','{\"X\":2,\"Y\":-667, \"Z\":16}',0,1,-1,-1,0),(24,'Pillbox Hospital','YMAP','Pillbox Hospital','RC12B_Default','{\"X\":307,\"Y\":-590, \"Z\":43}',1,1,-1,-1,0),(25,'Lost\'s Trailer Park','YMAP','Lost\'s Trailer Park','methtrailer_grp1','{\"X\":49,\"Y\":3744, \"Z\":46}',1,1,-1,-1,0),(26,'Red Hill Valley church - Grave','YMAP','Red Hill Valley church - Grave','lr_cs6_08_grave_closed','{\"X\":-282,\"Y\":2835, \"Z\":55}',1,1,-1,-1,0),(27,'Carwash','YMAP','Carwash','Carwash_with_spinners','{\"X\":55,\"Y\":-1391, \"Z\":30}',1,1,-1,-1,0),(28,'Ferris Wheel','YMAP','Ferris Wheel','ferris_finale_Anim','{\"X\":-1645,\"Y\":-1113, \"Z\":12}',1,1,-1,-1,0),(29,'Trevors Trailer (Clean)','YMAP','Trevors Trailer (Clean)','trevorstrailertidy','{\"X\":1975,\"Y\":3820, \"Z\":33}',1,1,-1,-1,0),(30,'Lost Clubhouse','YMAP','Lost Clubhouse','bkr_bi_hw1_13_int','{\"X\":984,\"Y\":-95, \"Z\":74}',1,1,-1,-1,0),(31,'Casino Penthouse Glassfront','YMAP','Casino Penthouse Glassfront','hei_dlc_windows_casino','{\"X\":968,\"Y\":0, \"Z\":111}',1,1,-1,-1,0),(32,'Tunnel','YMAP','Tunnel','v_tunnel_hole','{\"X\":-49,\"Y\":-558, \"Z\":30}',1,1,-1,-1,0),(33,'Tunnel','YMAP','Tunnel','v_tunnel_hole_lod','{\"X\":-49,\"Y\":-558, \"Z\":30}',1,1,-1,-1,0),(34,'Sandy Shores River','YMAP','Sandy Shores River','cs3_05_water_grp1	','{\"X\":-460,\"Y\":4429, \"Z\":27}',1,1,-1,-1,0),(35,'Sandy Shores River','YMAP','Sandy Shores River','cs3_05_water_grp1_lod','{\"X\":-460,\"Y\":4429, \"Z\":27}',0,0,-1,-1,0),(36,'Fort Hobo','YMAP','Fort Hobo','hobo_fort_walls','{\"X\":464,\"Y\":-854, \"Z\":26}',1,1,-1,-1,0),(37,'Vinewood Haus','YMAP','Vinewood Haus','bh1_47_joshhse_unburnt','{\"X\":-1177,\"Y\":302, \"Z\":66}',1,1,-1,-1,0),(38,'MLO_COURT_HOUSE','INTERIOR','Gerichtsgebäude','mlo_courthouse_milo_','{\"X\":241,\"Y\":-1091, \"Z\":32}',1,1,-1,-1,0),(39,'PALETO_SHERIFFS_OFFICE','INTERIOR','Sheriff Department ','paletosheriff_milo_','{\"X\":-458.86154,\"Y\":5992.945, \"Z\":31.268188}',1,1,-1,-1,0),(40,'CITY_BIKES','INTERIOR','City Bikes','notixx_citybikes_milo_','{\"X\":-193.38242,\"Y\":-37.345055, \"Z\":50.679077}\r\n\r\n',1,1,-1,-1,0),(41,'KRNT','INTERIOR','KRNT Bar','gta5interiors_krnt_milo_','{\"X\":378.71,\"Y\":-1077.216,\"Z\":30}',1,1,-1,-1,0),(42,'DEL_PIERRO_HEIGHTS_ARPARTMENTS_1','YMAP','Del Pierro Heights Arpartment 1','mpbusiness_int_placement_interior_v_mp_apt_h_01_milo_','{\"X\":-1465.3187,\"Y\":-536.38684,\"Z\":73.47693}',0,0,-1,-1,0),(43,'DEL_PIERRO_HEIGHTS_ARPARTMENTS_2','YMAP','Del Pierro Heights Arpartment 2','hei_hw1_blimp_interior_28_dlc_apart_high2_new_milo_','{\"X\":-1465.3187,\"Y\":-536.38684,\"Z\":73.47693}',0,0,-1,-1,0),(44,'DEL_PIERRO_HEIGHTS_ARPARTMENTS_3','YMAP','Del Pierro Heights Arpartment 3','hei_hw1_blimp_interior_26_dlc_apart_high_new_milo_','{\"X\":-1465.3187,\"Y\":-536.38684,\"Z\":73.47693}',0,0,-1,-1,0),(45,'DEL_PIERRO_HEIGHTS_ARPARTMENTS_4','YMAP','Del Pierro Heights Arpartment 4','hei_hw1_blimp_interior_27_dlc_apart_high_new_milo_','{\"X\":-1465.3187,\"Y\":-536.38684,\"Z\":73.47693}',0,0,-1,-1,0),(46,'SAINTS_GARAGE','INTERIOR','Saints Garage','saints_garage_milo_','{\"X\":938.46594,\"Y\":-1478.6241,\"Z\":30.290894}',1,1,-1,-1,0),(57,'AOD_CLUBHOUSE','INTERIOR','AoD Clubhouse','rfc_mcbikers_milo_','{\"X\":2514.7122,\"Y\":4102.47,\"Z\":35.581665}',1,1,-1,-1,0),(58,'SANITATION_SGE','YMAP','Sanitation Sortiermaschine','sanitation_sorting_machine','{\"X\":-504.64,\"Y\":-1734.41,\"Z\":22.75}',1,1,-1,-1,0),(63,'UNION_DEPOSITY','YMAP','UNION_DEPOSITY','dt1_03_shutter','{\"X\":-6,\"Y\":-666,\"Z\":32}',1,1,-1,-1,0),(64,'BAUSTELLE_EAST_HIGHWAY','YMAP','Baustelle East Highway','baustelle_east_highway','{\"X\":1691.35229,\"Y\":1373.34167,\"Z\":86.11872}',0,1,-1,-1,0),(69,'BAUSTELLE_CITY_BIKES','YMAP','Baustelle City Bikes','baustelle_city_bikess','{\"X\":-253.4567,\"Y\":-47.23034,\"Z\":48.6558456}',1,1,-1,-1,0),(70,'BAUSTELLE_PDM','YMAP','Baustelle PDM','baustelle_pdm','{\"X\":-80.08454,\"Y\":-1087.711,\"Z\":25.5971069}',0,1,-1,-1,0),(71,'BAUSTELLE_ROUTE68_01','YMAP','Baustelle Route 68 01','baustelle_route68_eins','{\"X\":-1291.076,\"Y\":2512.583,\"Z\":20.7786369}',0,1,-1,-1,0),(72,'BAUSTELLE_SANDY','YMAP','Baustelle Sandy','baustelle_sandy_eins','{\"X\":1727.01428,\"Y\":3525.65552,\"Z\":36.26465}',1,1,-1,-1,0),(73,'BAUSTELLE_STADT_03','YMAP','Baustelle Stadt 03','baustelle_stadt_dre','{\"X\":-797.9808,\"Y\":232.776779,\"Z\":75.73761}',1,1,-1,-1,0),(74,'BAUSTELLE_STADT_01','YMAP','Baustelle Stadt 01','baustelle_stadt_eins','{\"X\":319.176178,\"Y\":-454.460876,\"Z\":42.3830032}',1,1,-1,-1,0),(75,'BAUSTELLE_STADT_02','YMAP','Baustelle Stadt 02','baustelle_stadt_zwei','{\"X\":1163.08069,\"Y\":-579.9631,\"Z\":464.11979}',1,1,-1,-1,0),(76,'BAUSTELLE_WESTHIGHWAY_01','YMAP','Baustelle Westhighway 02','baustelle_west_highway_eins','{\"X\":-2461.10059,\"Y\":-219.06311,\"Z\":17.2082329}',1,1,-1,-1,0),(77,'BAUSTELLE_SANITATION','YMAP','Baustelle Sanitation','baustelle_mullmenschen','{\"X\":-665.8013,\"Y\":-1668.29492,\"Z\":25.0176411}',0,1,-1,-1,0),(78,'MICHAELS_HOUSE','INTERIOR','Michaels House','hei_bh1_48_interior_v_michael_milo_','{\"X\":-810.76483,\"Y\":180.1055,\"Z\":72.14575}',1,1,NULL,NULL,0);
/*!40000 ALTER TABLE `configipls` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:32
