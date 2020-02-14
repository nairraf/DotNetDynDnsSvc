
# DotNetDynDnsService

DotNetDynDnsService allows you to dynamically update DNS A resource records served by a windows server runninng the DNS server role simply by accessing a URL from any end-point.

Many Dynamic DNS clients can be used, like the ones bundled with some open source firewalls like PFSense.

## Requirements

### DotNetDynDnsServer server requirements

* A windows server running the IIS Role capable of running .Net Framework 4.7.2
* TLS must be used between the client and the server. If terminating the TLS session on IIS, then configure IIS to only accept HTTPS connections for this site. 
    * You can use self signed if needed, as long as you configure the client to trust it.
    * since basic authentication is used, HTTPS must be configured on the IIS site, and HTTP should be rejected.
    * SSL/TLS Offloading with a load-balancer can be used. In this case terminal the TLS session on the load-balancer, and then send HTTP through the private network/dmz 
      to the back-end web server(s) running DotNetDynDnsServer.
* If put behind a load-balancer or CDN, then the HTTP header which has the real client IP should be configured in the config file.
* This site should not run on the DNS server, but on another server which has the ability to communicate over the network with the desired DNS server.
* WMI is used to remotely update the defined DNS server
* A local or domain account can be used to delegate DNS administration. This account must be permitted to update DNS records on the defined DNS server.
    * The password for the account must be encyprted in the configuration file using a certificate. Clear-Text is not supported.
    * only computers with that certificate's private key will be able to decrypt the password
    * if you don't want to use a certificate, then a complex seed can be used instead. but this is less secure as anyone that has access to the configuration file will be able to decrypt the password.
      this should only be used for testing with test accounts. Certificates should be used in production.

### DotNetDynDnsServer client requirements

* Basic Authentication over HTTPS (simple username and password)
* Issue an HTTP Get request to the URL the server is published with
    * Example: https://[serverURL]/default.aspx?action=updatedns

## How it works

The server accepts incoming HTTP Get requests and validates that the basic authentication fields are present (username and password). 
It then validates the credentials against a small SQLite database. The password field is used more like a token, where a single combination of username and password defines which A record can be updated.
If the username and password combination is valid, a DNS update using WMI is attempted against the defined DNS server for that specific resource record.

Multiple validation checks are done to ensure that only a specific DNS record updated is permitted, and that any malformed request is simply discarded. If the current IP Address of the A record matches the client IP of the HTTP request, no update is performed.

By default if you just hit https://[sercerurl]/defauls.aspx, you will get a 403, access is denied. This is normal as anonymous browsing is not permitted, and if there are no basic authentication headers present, the request is immediately terminated.

## Installation Instructions

See the Documentation folder

## Certificate Notes

Please note that only CSG certificates are supported. PowerShell (New-SelfSignedCertificate) creates CNG certificates by default and as such cannot be used.

Follow the below instructions to create a new CSG certificate using OpenSSL. This can be done on any OS that has OpenSSL installed.

### Create a self-signed cert for data encryption purposes

You can replace the name "DotNetDynDnsSvc Security Certificate" in the below command to anything you want. You will just have to update the config file to reference the new name
Adjust the -days option to the amount of days/years  you want the certificate to be valid for.

This will create a new private key (key.pem) and public certificate (certificate.pem). You will be prompted for a password for the private key.

```
openssl req -newkey rsa:2048 -keyout key.pem -x509 -days 365 -out certificate.pem -subj "/CN=DotNetDynDnsSvc Security Certificate"
```

Now we will take the private key and public certificate and combine them in a PFX archive.

```
openssl pkcs12 -inkey key.pem -in certificate.pem -export -out DotNetDynDnsSvc_Security_Certificate.pfx
```

you will be prmpted to enter the private key password, as well as a new password for the PFX archive. You will need this password whenever you import the PFX archive anywhwere.

When importing the PFX archive, it is important to mark the private key as exporttable, and that you import it into the local machine store. See the example in the documentation folder/documents.

you can now delete the certificate.pem and key.pem files, as they are embeded in the PFX archive.