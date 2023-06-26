# OCore C4 model support

This library provides support for the C4 model (https://c4model.com/) in OCore.

Some ideas:

- Implicit mappings
  - Map roles to C4 person
  - Map root namespace to Software System
  - Map name with wild cards to Container
  - Map Service/DataEntity to Component
  - Map components using call diagnostics from test run, perhaps?
- Attributes
  - C4PersonAttribute
  - C4SoftwareSystemAttribute
  - C4ContainerAttribute
  - C4ComponentAttribute
  - C4RelationshipAttribute