# NHibernate Mapping Generator

A simple utility to generate NHibernate mapping files and corresponding domain classes from existing DB tables.

Features:
- Supports Oracle, SqlServer, PostgreSQL, MySQL, SQLite, Sybase, Ingres, CUBRID
- Can generate hbm.xml, Fluent NHibernate and NH 3.3 Fluent style of mapping files.
- Has lots of preferences to control the property naming conventions.
- Can generate Domain Entity and WCF Data Contracts too.
- Can generate one table at a time or script entire DB in one go. (It can generate mapping for around 800 tables in under 3 minutes on my moderately powered laptop)
- Supports ActiveRecord code generation.
- Its super fast and free. No licensing restrictions.
- Option to generate NHibernate or MS validators

[![Build Status](https://travis-ci.org/rvrn22/nmg.svg?branch=master)](https://travis-ci.org/rvrn22/nmg)

## Tutorial:

CUBRID : http://www.cubrid.org/wiki_apis/entry/using-nmg-nhibernate-mappings-generator-with-cubrid
