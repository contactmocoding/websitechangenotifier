# websitechangenotifier

Identify all the pages found
If not in the list, flag it as new.
If new, store URL, and the Checksum of the page content
If not new, create checksum, if different to last time, then send the content change email.

Can this be done in a time based Azure Function?
