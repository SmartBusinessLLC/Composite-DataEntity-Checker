![Smart Bird CDEC](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/SmartBirdCDEC.png)

# Composite-DataEntity-Checker
Composite DataEntity Checker is a D365FO Visual Studio addin that provides user with functionality to check Data Entity if it was configured accordingly to Microsoft Best Practices
# What Composite DataEntity Checker can check?
1. Check if there are Staging tables for Data Entity tables
2. If there are Parent and Child Data Entities
3. Relation between Parent and Child DataEnitites
4. Relation between Staging tables and Child DataEnitites
5. Existence of mandatory fields in Staging table
6. Existence of indexes with mandatory field and field connecting Staging tables
7. Existence of relations on tables, using which Parent and Child DataEntities were created.
# How to use Composite DataEnityt Checker?

1. Open designer

![Open designer](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/1.png)

2. Right click on Composite Data Entity -> Check Composite Data Entity

![Check Composite Data Entity](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/2.png)

3. Data Entity Checker mechanism is on, and now you will get a message with information on what you need to do with your Data Entity to complie Microsoft Best Practices

![Check Composite Data Entity](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/3.png)

<i>example: ConfiguratorEntity has some Best Practises deviations </i>

![Check Composite Data Entity](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/4.png)

![Check Composite Data Entity](https://raw.githubusercontent.com/SmartBusinessLLC/Composite-DataEntity-Checker/master/Images/5.png)

<i>The result shows that this DataEntity has missing relations between DataSources</i>

# Version
Was tesded on Environment:
Update11 (7.0.4679.35176)
Microsoft Dynamics 365 for Finance and Operations, Enterprise edition (July 2017)

Visual Studio:

Microsoft Visual Studio Professional 2015 Version 14.0.25420.01 Update 3
