﻿# Tables 2022-08-18

For a restaurant, we need to be able to take orders and have them settled. We need to manage tables, waiters and customers.

This is just a small sample to mimic that stupid bullshit you see in a lot of restaurants where a customer can order to a table instead of getting the real restaurant experience where they actually interact with a waiter.

Mostly, this is a test to see if generating a scaffold from an MD is a viable option, to see if we can reach the "coding at the speed of thought" ideal.

It will break the system down into the "common five pieces":

- Services - Something that responds quickly to a command
- Entities - Something that responds reasonably quickly to working with data
- Events - Something happened
- EventHandlers - Something that responds to events
- Exceptions - Something that is thrown when something goes wrong

If you imagine this is a document you can create while spending 30 minutes with product designers and stakeholders, you will be doing it correctly.

I am currently doing this to see if it is at all valuable. There will be no backwards updating of this document, but you can hopefully use it as a combination of a physical meeting where you take some notes and perhaps for code generation.

GOAL: 

"When will it be done?"

"I did it while we talked about it."

## Services

- Customer
    * New table => Table - Hihi, these are similar to check in/check out for the waiter
        - Fails with : Restaurant is full
    * Adieu - The customer is settled and wants to go home
- Waiter
    - Check in : Waiter
    - Check out : Waiter

## Entities

- Table
    - Number - "The customer at table number 4 needs another shot of Vaccine" Does it make sense that "Number" is recognized as an int automatically? Perhaps make some assumptions on name of the field? I feel this will be using Humanizer-library heavily
    - Settled : bool
- Customer - The customer is probably anonymous in this case
    * Settle : Settlement
        - Fails with
            - Insufficient funds
- Order
    - Table - It is linked like this because an order could move from one table to another
    - Total, summed Item : decimal
    - Settlements - Multiple settlements for same order is possible
    - Settled? - Question mark indicates it is a bool (Make a "Settle"-method?)
    - Orderlines - Plural of existing entity makes it a list linked to another entity
    - Taken by : Waiter
    - Delivered by : Waiter
- Orderline
    - Item
    - Amount : int    
- Item
    - Name
    - Price : decimal
    - PictureUrl
    - Delivered?
- Settlement
    - Order
    - SettledBy : Customer
    - Tip : decimal - Waiters need love too
- Waiter
    - Name

## Events

- New order - "I feel so extraordinary"
    - Order
- Order settled
    - Order

## Exceptions

- Restaurant is full - "Sorry sir and or madame, it is full"
- Insufficient funds - "You don't seem to be able to pay for yourself, you drunken fool."
