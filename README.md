# Mercury
Simple stream consumer application

## Simple Architecture
I know it's far from a standard architecture, but that can give a basic overview of how the application is implemented.

![Mercury](https://github.com/rafaborges/Mercury/raw/master/Documents/mercury%20architecture.png)

The IBufferPersistence has not yet been uploaded as it still not working as it should.

## Services
### Kafka
The Kafka consumer expects two parameter: host and topic. You can only add one topic per chart. It registers itself under a group id called mercury-consumer.

### Azure
Ready, but poorly tested. This consumer expects four parameters:
 * connectionString
 * entityPath
 * storageConnectionString
 * storageContainer

### Random
This is a simple random generator. It push data at a rate of one random double per second. Data ranges from 0 to 1.

## Known Issues
 * SQL Server buffer persistence has not been tested yet.
 * Write now saving view configuration while debugging write files to C:\Program Files (x86)\IIS Express. In the fure the path will be stored in a config files
 * The same happens to XML buffer persistence.