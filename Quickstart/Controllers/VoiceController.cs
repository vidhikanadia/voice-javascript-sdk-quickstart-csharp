﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Quickstart.Models.Configuration;
using Quickstart.Models;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using System.Text.RegularExpressions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio;

namespace Quickstart.Controllers
{
	public class VoiceController : Controller
	{
		#region fields
		private readonly TwilioAccountDetails _twilioAccountDetails;
		#endregion

		#region properties
		public VoiceResponse twiml { get; set; } = new VoiceResponse();
		#endregion

		#region constructor
		public VoiceController(IOptions<TwilioAccountDetails> twilioAccountDetails)
		{
			_twilioAccountDetails = twilioAccountDetails.Value ?? throw new ArgumentException(nameof(twilioAccountDetails));
		}
		#endregion

		#region methods
		// POST: /voice
		[HttpPost]
		public IActionResult Index(string to, string callingDeviceIdentity)
		{
			var callerId = _twilioAccountDetails.CallerId;

			Console.WriteLine($"to: {to}, callingDeviceIdentity: {callingDeviceIdentity}, thisDevice.Identity: {Device.Identity}");

			// someone calls into my Twilio Number, there is no thisDeviceIdentity passed to the /voice endpoint 
			if (string.IsNullOrEmpty(callingDeviceIdentity))
			{
				var dial = new Dial();
				var client = new Twilio.TwiML.Voice.Client();
				client.Identity(Device.Identity);
				dial.Append(client);
				twiml.Append(dial);
			}
			else if (callingDeviceIdentity != Device.Identity)
			{
				var dial = new Dial();
				var client = new Twilio.TwiML.Voice.Client();
				client.Identity(Device.Identity);
				dial.Append(client);
				twiml.Append(dial);
			}
			// if the POST request contains your browser device's identity
			// make an outgoing call to either another client or a number
			else
			{
				var dial = new Dial(callerId: callerId); //for recoding: record: Dial.RecordEnum.RecordFromAnswer

				// check if the 'To' property in the POST request is
				// a client name or a phone number
				// and dial appropriately using either Number or Client

				if (Regex.IsMatch(to, "^[\\d\\+\\-\\(\\) ]+$"))
				{
					Console.WriteLine("Match is true");
					dial.Number(to);
				}
				else
				{
					var client = new Twilio.TwiML.Voice.Client();
					client.Identity(to);
					dial.Append(client);

				}
				twiml.Append(dial);
			}

			Console.WriteLine(twiml.ToString());

			return Content(twiml.ToString(), "text/xml");
		}

		[HttpPost("voice/placeonhold")]
		public IActionResult PlaceCallOnHold([FromBody] CallData requestData)
		{
			string callSid = requestData.CallSid;

			TwilioClient.Init(_twilioAccountDetails.AccountSid, _twilioAccountDetails.AuthToken);

			//var twiml = new VoiceResponse();
			var enqueue = new Enqueue("support", waitUrl: new Uri("http://com.twilio.sounds.music.s3.amazonaws.com/MARKOVICHAMP-Borghestral.mp3"));
			twiml.Append(enqueue);

			var call = CallResource.Update(
					twiml: new Twiml(twiml.ToString()),
					pathSid: callSid
			);

			return Content(twiml.ToString(), "text/xml");
		}
		#endregion
	}
}