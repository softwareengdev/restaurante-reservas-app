# Backend Domain Exercise – Restaurant Reservations

**Estimated time:** 3–4 hours  
**Focus:** Domain modeling, correctness, and tests. HTTP/persistence are optional bonuses.

---

## Problem
Implement a small system that can answer:

> “Which tables are available for a party of **N** at time **T** for **D** minutes?”

---

## Capabilities
Provide operations to:

- **Add a table** with a given capacity.  
- **Create a reservation** on a specific table.  
- **Cancel a reservation.**  
- **Query availability** for a `(partySize, start, duration)`.  

---

## Reservation Validity
A reservation is **valid if and only if**:

- The table can hold the requested party size.  
- It does not conflict with any other reservation on the same table.  

You must decide and document how to detect conflicts.

---

## Required Behaviours (show in tests)
Demonstrate these behaviours (with any data you choose):

1. A reservation that ends exactly when another begins on the same table is **allowed**.  
2. A reservation that overlaps another on the same table is **rejected**.  
3. A reservation exceeding a table’s capacity is **rejected**.  
4. An availability query returns **all and only** tables that can host the party without conflicts.  

---

## Deliverable
Submit either:  

- A **public Git repository link**, or  
- A **ZIP file** with the source code.  

Your submission must include:  

- **README**: how to build/run/tests, modeling choices, assumptions.  
- **Source code** implementing the capabilities.  
- **Automated tests** demonstrating the required behaviours.  

**Language:** Any (C# preferred). Keep dependencies minimal.
