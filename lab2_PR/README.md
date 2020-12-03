### status: Work in progress

### lab2_PR Project is based on the following requirements:

* Implement a protocol atop UDP, with error checking and retransmissions. Limit the number of retries for retransmission.
* Make the connection secure, using either a CA to get the public key of the receiver and encrypt data with it, or using Diffie-Helman to get a shared connection key between client and server, ensure that the traffic is encrypted.
* Regarding the application-level protocol, there are 3 options:
  - make an FTP-like protocol for data transfer, thus you will need to ensure data splitting and in-order delivery and reassembly at the destination. The protocol must support URIs, file creation and update (PUT), file fetching (GET) and metadata retrieval (OPTIONS)
  - make a protocol based on the workings (state machine) of an ATM
  - make a protocol based on the workings (state machine) of a stationary telephone
