'''
Author: Jacob Slaton

Requires two files.

A file named config.txt with an email address and password on separate lines.
The script logs into this email and uses it to send emails to each participant.

Example:
email example@email.com
password samplepassword

A file named names.txt with the email address and name of each
participant on separate lines.

Example:
jeff@email.com Jeff
smithfamily@email.com the Smiths
'''

import datetime
import hashlib
import random
import smtplib
import string

def salt_and_pepper(email):
	salt = hashlib.md5(str(datetime.datetime.now().year).encode()).hexdigest()
	pepper = random.choice(string.ascii_lowercase)
	return email+salt+pepper

def hash(email):
	return hashlib.md5(salt_and_pepper(email).encode()).hexdigest()[:8]

def verify(plaintext, ciphertext):
	salt = hashlib.md5(str(datetime.datetime.now().year).encode()).hexdigest()
	for ii in string.ascii_lowercase:
		if hashlib.md5((plaintext+salt+ii).encode()).hexdigest()[:8] == ciphertext:
			return True
	return False

def send(email, password, receiver, message):
	server = smtplib.SMTP("smtp.gmail.com:587")
	server.starttls()
	server.login(email, password)
	server.sendmail(email, receiver, message)
	server.quit()

# Email credentials
email = ""
password = ""
with open("config.txt", "r") as fin:
	for line in fin:
		if line.startswith("email "):
			email = line[6:]
		elif line.startswith("password "):
			password = line[9:]

# Email template
subject = "Secret Snowman"
message = """
You are buying for {}.

Confirmation Code: {}
"""[1:]
message = "Subject: {}\n\n{}".format(subject, message)

# Read and assign names
names = {}
emails = []
with open("names.txt", "r") as fin:
	for line in fin:
		if not line in ("", "\n"):
			names[line[:line.find(" ")]] = line[line.find(" ")+1:-1]
			emails.append(line[:line.find(" ")])
random.shuffle(emails)

hashes = {ii:hash(ii) for ii in emails}
partners = {ii:emails[(emails.index(ii)+1)%len(emails)] for ii in emails}

confirmation = "Subject: {}\n\n".format("Secret Snowman Confirmation Codes")
confirmation += """
Share this list with everyone in your Secret Snowman and verify that everyone's confirmation codes match with the corresponding code on this list.

"""[1:]
for key, val in partners.items():
	confirmation += "{} : {}\n".format(hashes[val], key)

# Send emails
sendemail, password, "throwaway.email.bot@gmail.com", confirmation)
for key, val in partners.items():
	send(email, password, key, message.format(names[val], hashes[val]))
