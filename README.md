
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

### DotNetDynDnsServer client requirements

* Basic Authentication over HTTPS (simple username and password)
* Issue an HTTP Get request to the URL the server is published with
    * Example: https://<serverurl>/default.aspx?action=updatedns

## How it works

The server accepts incoming HTTP Get requests and validates that the basic authentication fields are present (username and password). 
It then validates the credentials against a small SQLite database. The password field is used more like a token, where a single combination of username and password defines which A record can be updated.
If the username and password combination is valid, a DNS update using WMI is attempted against the defined DNS server for that specific resource record.

Multiple validation checks are done to ensure that only a specific DNS record updated is permitted, and that any malformed request is simply discarded.

By default if you just hit https://<sercerurl>/defauls.aspx, you will get a 403, access is denied. This is normal as anonymous browsing is not permitted, and if there are no basic authentication headers present, the request is immediately terminated.

## Instructions

See the Documentation folder