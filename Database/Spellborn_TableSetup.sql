
SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for `accounts`
-- ----------------------------
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL DEFAULT '',
  `Pass` varchar(255) NOT NULL DEFAULT '',
  `Email` varchar(255) NOT NULL DEFAULT '',
  `banned` int(11) NOT NULL DEFAULT '0',
  `Level` int(11) NOT NULL DEFAULT '0',
  `LastLogin` varchar(255) NOT NULL DEFAULT '',
  `IsOnline` int(11) NOT NULL,
  `SessionKey` int(11) NOT NULL DEFAULT '-1',
  `LastUniverse` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`,`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `devnotes`
-- ----------------------------
DROP TABLE IF EXISTS `devnotes`;
CREATE TABLE `devnotes` (
  `CharacterID` int(11) NOT NULL,
  `Position` varchar(255) NOT NULL,
  `Note` varchar(255) NOT NULL,
  `ZoneID` int(11) NOT NULL DEFAULT '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `knownuniverses`
-- ----------------------------
DROP TABLE IF EXISTS `knownuniverses`;
CREATE TABLE `knownuniverses` (
  `ID` int(11) NOT NULL,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`ID`,`Name`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `playercharacteritems`
-- ----------------------------
DROP TABLE IF EXISTS `playercharacteritems`;
CREATE TABLE `playercharacteritems` (
  `CharacterID` int(11) NOT NULL,
  `ID` int(11) NOT NULL DEFAULT '0',
  `ResourceID` int(11) NOT NULL,
  `Stacks` int(11) NOT NULL DEFAULT '1',
  `LocationType` int(11) NOT NULL DEFAULT '1',
  `LocationSlot` int(11) NOT NULL,
  `Attuned` int(11) NOT NULL DEFAULT '0',
  `Color1` int(11) NOT NULL DEFAULT '0',
  `Color2` int(11) NOT NULL DEFAULT '0',
  `Serial` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`CharacterID`,`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `playercharacters`
-- ----------------------------
DROP TABLE IF EXISTS `playercharacters`;
CREATE TABLE `playercharacters` (
  `AccountID` int(11) NOT NULL,
  `CharacterID` int(11) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Appearance` varchar(255) NOT NULL DEFAULT '0',
  `LastMapID` int(11) NOT NULL,
  `Faction` int(11) NOT NULL,
  `ArcheType` int(11) NOT NULL DEFAULT '0',
  `PawnState` int(11) NOT NULL,
  `Position` varchar(255) NOT NULL DEFAULT '(0,0,0)',
  `Rotation` varchar(255) NOT NULL DEFAULT '(0,0,0)',
  `FamePep` varchar(255) NOT NULL DEFAULT '1,0',
  `HealthMaxHealth` varchar(255) NOT NULL DEFAULT '100/100',
  `BMF` varchar(255) NOT NULL DEFAULT '10,10,10',
  `PMC` varchar(255) NOT NULL DEFAULT '100,10,10',
  `Money` int(11) NOT NULL DEFAULT '0',
  `BMFAttributeExtraPoints` varchar(255) NOT NULL DEFAULT '0,0,0,0',
  `SkillDeck` varchar(255) NOT NULL DEFAULT '0#',
  PRIMARY KEY (`AccountID`,`CharacterID`),
  UNIQUE KEY `CharacterID` (`CharacterID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `playercharacterskills`
-- ----------------------------
DROP TABLE IF EXISTS `playercharacterskills`;
CREATE TABLE `playercharacterskills` (
  `CharacterID` int(11) NOT NULL,
  `SkillID` int(11) NOT NULL DEFAULT '0',
  `SigilSlots` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`CharacterID`,`SkillID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for `server`
-- ----------------------------
DROP TABLE IF EXISTS `server`;
CREATE TABLE `server` (
  `SupportedClientVersion` int(11) NOT NULL,
  UNIQUE KEY `SupportedClientVersion` (`SupportedClientVersion`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of server
-- ----------------------------
INSERT INTO server VALUES ('28430');
