CREATE USER SCOTT IDENTIFIED BY tiger 
DEFAULT TABLESPACE "USERS"
TEMPORARY TABLESPACE "TEMP";

-- ROLES
GRANT "SELECT_CATALOG_ROLE" TO SCOTT ;
GRANT "CONNECT", "RESOURCE" TO SCOTT ;

-- SYSTEM PRIVILEGES
GRANT SELECT ANY DICTIONARY TO SCOTT;

CREATE TABLE SCOTT.CATEGORIES
(
  ID NUMBER(9),
  NAME NVARCHAR2(255) NOT NULL,
  CONSTRAINT CATEGORIES_PK PRIMARY KEY (ID)
);

CREATE TABLE SCOTT.PRODUCTS
(
  ID NUMBER(16),
  NAME NVARCHAR2(255) NOT NULL,
  CATEGORY_ID NUMBER(9),
  CONSTRAINT PRODUCTS_PK PRIMARY KEY (ID),
  CONSTRAINT PROD_CATEG_FK FOREIGN KEY (CATEGORY_ID) REFERENCES CATEGORIES(ID)
);

CREATE TABLE SCOTT.STORES
(
  ID NUMBER (9),
  NAME NVARCHAR2(255) NOT NULL,
  DESCRIPTION NVARCHAR2(1000),
  CONSTRAINT STORES_PK PRIMARY KEY (ID)
);

CREATE TABLE SCOTT.INVENTORIES
(
  ID NUMBER (18),
  STORE_ID NUMBER(9) NOT NULL,
  PRODUCT_ID NUMBER(16) NOT NULL,
  QUANTITY NUMBER(16,3) NOT NULL,
  ADDED_AT DATE NOT NULL,
  MODIFIED_AT DATE NULL,
  CONSTRAINT INVENTORIES_PK PRIMARY KEY (ID),
  CONSTRAINT INVEN_STORE_FK FOREIGN KEY (STORE_ID) REFERENCES STORES(ID),
  CONSTRAINT INVEN_PROD_FK FOREIGN KEY (PRODUCT_ID) REFERENCES PRODUCTS(ID)
);

CREATE SEQUENCE SCOTT.CATEGORY_SEQ
INCREMENT BY 1 
START WITH 1 
MAXVALUE 99999999999 
MINVALUE 1 
NOCACHE;

CREATE SEQUENCE SCOTT.PRODUCT_SEQ
INCREMENT BY 1 
START WITH 1 
MAXVALUE 99999999999 
MINVALUE 1 
NOCACHE;

CREATE SEQUENCE SCOTT.STORE_SEQ
INCREMENT BY 1 
START WITH 1 
MAXVALUE 99999999999 
MINVALUE 1 
NOCACHE;

CREATE SEQUENCE SCOTT.INVENTORY_SEQ
INCREMENT BY 1 
START WITH 1 
MAXVALUE 99999999999 
MINVALUE 1 
NOCACHE;

/* Type Mapping */

/*
BINARY_DOUBLE                                         Double
BINARY_FLOAT                                          Single
BINARY_INTEGER                                        Decimal
BLOB                                                  Byte[]
CHAR                                                  String
CLOB                                                  String
DATE                                                  DateTime
FLOAT                                                 Decimal
INTERVAL DAY TO SECOND                                TimeSpan
INTERVAL YEAR TO MONTH                                Int64
NCHAR                                                 String
NCLOB                                                 String
NVARCHAR2(1000)                                       String
REAL                                                  Decimal
ROWID                                                 String
TIMESTAMP                                             DateTime
TIMESTAMP WITH LOCAL TIME ZONE                        DateTime
TIMESTAMP WITH TIME ZONE                              DateTime
UROWID                                                String
VARCHAR2(1000)                                        String
RAW                                                   Byte[]
BFILE *                                               Byte[]

number(1, 0)    Int16
number(2, 0)    Int16
number(2, 1)    Single
number(3, 0)    Int16
number(3, 1)    Single
number(3, 2)    Single
number(4, 0)    Int16
number(4, 1)    Single
number(4, 2)    Single
number(4, 3)    Single
number(5, 0)    Int32
number(5, 1)    Single
number(5, 2)    Single
number(5, 3)    Single
number(5, 4)    Single
number(6, 0)    Int32
number(6, 1)    Single
number(6, 2)    Single
number(6, 3)    Single
number(6, 4)    Single
number(6, 5)    Single
number(7, 0)    Int32
number(7, 1)    Single
number(7, 2)    Single
number(7, 3)    Single
number(7, 4)    Single
number(7, 5)    Single
number(7, 6)    Single
number(8, 0)    Int32
number(8, 1)    Double
number(8, 2)    Double
number(8, 3)    Double
number(8, 4)    Double
number(8, 5)    Double
number(8, 6)    Double
number(8, 7)    Double
number(9, 0)    Int32
number(9, 1)    Double
number(9, 2)    Double
number(9, 3)    Double
number(9, 4)    Double
number(9, 5)    Double
number(9, 6)    Double
number(9, 7)    Double
number(9, 8)    Double
number(10, 0)    Int64
number(10, 1)    Double
number(10, 2)    Double
number(10, 3)    Double
number(10, 4)    Double
number(10, 5)    Double
number(10, 6)    Double
number(10, 7)    Double
number(10, 8)    Double
number(10, 9)    Double
number(11, 0)    Int64
number(11, 1)    Double
number(11, 2)    Double
number(11, 3)    Double
number(11, 4)    Double
number(11, 5)    Double
number(11, 6)    Double
number(11, 7)    Double
number(11, 8)    Double
number(11, 9)    Double
number(11, 10)    Double
number(12, 0)    Int64
number(12, 1)    Double
number(12, 2)    Double
number(12, 3)    Double
number(12, 4)    Double
number(12, 5)    Double
number(12, 6)    Double
number(12, 7)    Double
number(12, 8)    Double
number(12, 9)    Double
number(12, 10)    Double
number(12, 11)    Double
number(13, 0)    Int64
number(13, 1)    Double
number(13, 2)    Double
number(13, 3)    Double
number(13, 4)    Double
number(13, 5)    Double
number(13, 6)    Double
number(13, 7)    Double
number(13, 8)    Double
number(13, 9)    Double
number(13, 10)    Double
number(13, 11)    Double
number(13, 12)    Double
number(14, 0)    Int64
number(14, 1)    Double
number(14, 2)    Double
number(14, 3)    Double
number(14, 4)    Double
number(14, 5)    Double
number(14, 6)    Double
number(14, 7)    Double
number(14, 8)    Double
number(14, 9)    Double
number(14, 10)    Double
number(14, 11)    Double
number(14, 12)    Double
number(14, 13)    Double
number(15, 0)    Int64
number(15, 1)    Double
number(15, 2)    Double
number(15, 3)    Double
number(15, 4)    Double
number(15, 5)    Double
number(15, 6)    Double
number(15, 7)    Double
number(15, 8)    Double
number(15, 9)    Double
number(15, 10)    Double
number(15, 11)    Double
number(15, 12)    Double
number(15, 13)    Double
number(15, 14)    Double
number(16, 0)    Int64
number(17, 0)    Int64
number(18, 0)    Int64

*/