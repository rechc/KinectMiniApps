using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SkyBiometry.Client.FC
{
	/// <summary>
	/// Result of a <see cref="FCClient"/> method call.
	/// </summary>
	[DataContract(Name = "response", Namespace = "")]
	public sealed class FCResult
	{
		#region Public types

		/// <summary>
		/// Specialized namespace to user list dictionary with custom serialization.
		/// </summary>
		public sealed class NamespaceUsersDictionary : Dictionary<string, List<string>>, IXmlSerializable
		{
			#region Public methods

			/// <summary>
			/// Generates an object from its XML representation.
			/// </summary>
			/// <param name="writer">The <see cref="XmlWriter"/> stream to which the object is serialized.</param>
			public void WriteXml(XmlWriter writer)
			{
				foreach (KeyValuePair<string, List<string>> keyValuePair in this)
				{
					writer.WriteStartElement(keyValuePair.Key, string.Empty);
					foreach (var str in keyValuePair.Value)
					{
						writer.WriteElementString("string", string.Empty, str);
					}
					writer.WriteEndElement();
				}
			}

			/// <summary>
			/// Converts an object into its XML representation.
			/// </summary>
			/// <param name="reader">The <see cref="XmlReader"/> stream from which the object is deserialized.</param>
			public void ReadXml(XmlReader reader)
			{
				var stringArr = new List<string>();

				bool startedGlobal = false;
				string nameTemp = string.Empty;

				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && !startedGlobal)
					{
						nameTemp = reader.Name;
						startedGlobal = true;
					}

					if (reader.NodeType == XmlNodeType.Text && startedGlobal)
					{
						stringArr.Add(reader.Value);
					}

					if (reader.NodeType == XmlNodeType.EndElement)
					{
						if (reader.Name == nameTemp && startedGlobal)
						{
							Add(nameTemp, stringArr);
							stringArr = new List<string>();
							startedGlobal = false;
						}
					}
				}
			}

			/// <summary>
			/// This method is reserved.
			/// </summary>
			/// <returns> Always returns <c>null</c>.</returns>
			public XmlSchema GetSchema()
			{
				return null;
			}

			#endregion
		}

		#endregion

		#region Public constants

		/// <summary>Specifies image decode error.</summary>
		public const int ErrorImageDecode = 20;
		/// <summary>Specifies image resizing error.</summary>
		public const int ErrorImageResize = 21;
		/// <summary>Specifies download error.</summary>
		public const int ErrorDownload = 30;
		/// <summary>Specifies download error: file was not found.</summary>
		public const int ErrorDownloadFileNotFound = 31;
		/// <summary>Specifies download error: server timed out.</summary>
		public const int ErrorDownloadServerTimeout = 32;
		/// <summary>Specifies download error: file is too large.</summary>
		public const int ErrorDownloadFileTooLarge = 33;
		/// <summary>Specifies download error: URL is malformed.</summary>
		public const int ErrorDownloadMalformedUrl = 34;
		/// <summary>Specifies download error: host is unknown.</summary>
		public const int ErrorDownloadUnknownHost = 35;
		/// <summary>Specifies download error: connection was refused.</summary>
		public const int ErrorDownloadConnectionRefused = 36;
		/// <summary>Specifies internal error.</summary>
		public const int ErrorInternal = 104;
		/// <summary>Specifies service temporarily unavailable error.</summary>
		public const int ErrorServiceTemporarilyUnavailable = 105;
		/// <summary>Specifies unknown error.</summary>
		public const int ErrorUnknown = 107;
		/// <summary>Specifies API key does not exist error.</summary>
		public const int ErrorApiKeyDoesNotExist = 201;
		/// <summary>Specifies API key usage passed queta error.</summary>
		public const int ErrorApiKeyUsagePassedQuota = 202;
		/// <summary>Specifies API key concurrent usage passed quota error.</summary>
		public const int ErrorApiKeyConcurrentUsagePassedQuota = 203;
		/// <summary>Specifies API key not authenticated error.</summary>
		public const int ErrorApiKeyNotAuthenticated = 204;
		/// <summary>Specifies API password not correct error.</summary>
		public const int ErrorApiPasswordNotCorrect = 205;
		/// <summary>Specifies maximum number of user ids trained in namespace exceeded error.</summary>
		public const int ErrorMaxNumberOfUserIdsTrainedInNamespaceExceeded = 206;
		/// <summary>Specifies too many errors error.</summary>
		public const int ErrorTooManyErrors = 207;
		/// <summary>Specifies tag not found error.</summary>
		public const int ErrorTagNotFound = 301;
		/// <summary>Specifies Facebook exception error.</summary>
		internal const int ErrorFacebookException = 302;
		/// <summary>Specifies filter syntax error.</summary>
		public const int ErrorFilterSyntax = 303;
		/// <summary>Specifies authorization error.</summary>
		public const int ErrorAuthorization = 304;
		/// <summary>Specifies Twitter exception error.</summary>
		internal const int ErrorTwitterException = 305;
		/// <summary>Specifies tag already exists error.</summary>
		public const int ErrorTagAlreadyExist = 306;
		/// <summary>Specifies action not permitted error.</summary>
		public const int ErrorActionNotPermitted = 307;
		/// <summary>Specifies unknown REST method error.</summary>
		public const int ErrorUnknwownRestMethod = 401;
		/// <summary>Specifies missing argument error.</summary>
		public const int ErrorMissingArgument = 402;
		/// <summary>Specifies missing user namespace error.</summary>
		public const int ErrorMissingUserNamespace = 403;
		/// <summary>Specifies unauthorized user namespace error.</summary>
		public const int ErrorUnauthorizedUserNamespace = 404;
		/// <summary>Specifies unauthorized user id error.</summary>
		public const int ErrorUnauthorizedUserId = 405;
		/// <summary>Specifies invalid argument value error.</summary>
		public const int ErrorInvalidArgumentValue = 406;
		/// <summary>Specifies argument list tool long error.</summary>
		public const int ErrorArgumentListTooLong = 407;
		/// <summary>Specifies unauthorized callback URL domain error.</summary>
		internal const int ErrorUnauthorizedCallbackUrlDomain = 408;
		/// <summary>Specifies user id too long error.</summary>
		public const int ErrorUserIdTooLong = 409;
		/// <summary>Specifies synchronous request too big error.</summary>
		internal const int ErrorSynchronousRequestTooBig = 410;

		#endregion

		/// <summary>
		/// The raw response from the API.
		/// </summary>
		/// <remarks>
		/// Available only if <c>true</c> is specified as getRawData in <see cref="FCClient.FCClient(string, string, string, Format, bool)"/> (<see cref="Format" /> controls the format of the data).
		/// </remarks>
		public string RawData { get; set; }

		/// <summary>
		/// List of one or more group of faces found in photos, when using <see cref="FCClient.FacesApi.BeginGroup"/>.
		/// </summary>
		[DataMember(Name = "groups", IsRequired = false, EmitDefaultValue = false, Order = 0)]
		public List<Group> Groups { get; set; }

		/// <summary>
		/// List of all the photo objects that were included in the request, one photo per URL or image stream specified.
		/// </summary>
		[DataMember(Name = "photos", IsRequired = false, EmitDefaultValue = false, Order = 1)]
		public List<Photo> Photos { get; set; }

		/// <summary>
		/// List of training statuses of the users (<see cref="FCClient.FacesApi.BeginStatus"/> only).
		/// </summary>
		[DataMember(Name = "user_statuses", IsRequired = false, EmitDefaultValue = false, Order = 2)]
		public List<UserStatus> UserStatuses { get; set; }

		/// <summary>
		/// List of users that were not trained (<see cref="FCClient.FacesApi.BeginTrain"/> only).
		/// </summary>
		[DataMember(Name = "no_training_set", IsRequired = false, EmitDefaultValue = false, Order = 3)]
		public List<UserStatus> NoTrainingSetUsers { get; set; }

		/// <summary>
		/// List of created users (<see cref="FCClient.FacesApi.BeginTrain"/> only).
		/// </summary>
		[DataMember(Name = "created", IsRequired = false, EmitDefaultValue = false, Order = 4)]
		public List<UserStatus> CreatedUsers { get; set; }

		/// <summary>
		/// List of updated users (<see cref="FCClient.FacesApi.BeginTrain"/> only).
		/// </summary>
		[DataMember(Name = "updated", IsRequired = false, EmitDefaultValue = false, Order = 5)]
		public List<UserStatus> UpdatedUsers { get; set; }

		/// <summary>
		/// List of unchanged users (<see cref="FCClient.FacesApi.BeginTrain"/> only).
		/// </summary>
		[DataMember(Name = "unchanged", IsRequired = false, EmitDefaultValue = false, Order = 6)]
		public List<UserStatus> UnchangedUsers { get; set; }

		/// <summary>
		/// Value specifying whether API usage is authenticated (<see cref="FCClient.AccountApi.BeginAuthenticate"/> only).
		/// </summary>
		[DataMember(Name = "authenticated", IsRequired = false, EmitDefaultValue = false, Order = 7)]
		[DefaultValue(false)]
		public bool IsAuthenticated { get; set; }

		/// <summary>
		/// List of namespaces along with list of users in each namespace (<see cref="FCClient.AccountApi.BeginUsers"/> only).
		/// </summary>
		[DataMember(Name = "users", IsRequired = false, EmitDefaultValue = false, Order = 8)]
		public NamespaceUsersDictionary Users { get; set; }

		/// <summary>
		/// List of namespaces (<see cref="FCClient.AccountApi.BeginNamespaces"/> only).
		/// </summary>
		[DataMember(Name = "namespaces", IsRequired = false, EmitDefaultValue = false, Order = 9)]
		public List<Namespace> Namespaces { get; set; }

		/// <summary>
		/// List of saved tags (<see cref="FCClient.TagsApi.BeginSave"/> only).
		/// </summary>
		[DataMember(Name = "saved_tags", IsRequired = false, EmitDefaultValue = false, Order = 10)]
		public List<SavedTag> SavedTags { get; set; }

		/// <summary>
		/// List of removed tags (<see cref="FCClient.TagsApi.BeginRemove"/> only).
		/// </summary>
		[DataMember(Name = "removed_tags", IsRequired = false, EmitDefaultValue = false, Order = 11)]
		public List<RemovedTag> RemovedTags { get; set; }

		/// <summary>
		/// Message from tag operation.
		/// </summary>
		[DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false, Order = 12)]
		public string Message { get; set; }

		/// <summary>
		/// The callback URL specified in request.
		/// </summary>
		[DataMember(Name = "callback_url", IsRequired = false, EmitDefaultValue = false, Order = 13)]
		internal string CallbackUrl { get; set; }

		/// <summary>
		/// Status of the request.
		/// </summary>
		public Status Status { get; set; }
		[DataMember(Name = "status", IsRequired = true, Order = 14)]
		private string _Status
		{
			get
			{
				return FCClient.EnumToString(Status);
			}
			set
			{
				Status = FCClient.EnumParse<Status>(value);
			}
		}

		/// <summary>
		/// Error code, specified only when an error occurs.
		/// </summary>
		[DataMember(Name = "error_code", Order = 15, IsRequired = false, EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int ErrorCode { get; set; }

		/// <summary>
		/// Error message, specified only when an error occurs.
		/// </summary>
		[DataMember(Name = "error_message", Order = 16, IsRequired = false, EmitDefaultValue = false)]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// The usage of the API. All rate limited methods and <see cref="FCClient.AccountApi.BeginLimits"/> return this object.
		/// </summary>
		[DataMember(Name = "usage", IsRequired = false, EmitDefaultValue = false, Order = 17)]
		public Usage Usage { get; set; }
	}

	/// <summary>
	/// Specifies status of the request.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Status
	{
		/// <summary>
		/// Specifies that the request has succeeded.
		/// </summary>
		[EnumMember(Value = "success")]
		Success,
		/// <summary>
		/// Specifies that the request has partially succeeded.
		/// </summary>
		[EnumMember(Value = "partial")]
		Partial,
		/// <summary>
		/// Specifies that the request has failed.
		/// </summary>
		[EnumMember(Value = "failure")]
		Failure
	}

	/// <summary>
	/// Specifies namespace share mode.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum NamespaceShareMode
	{
		/// <summary>
		/// Specifies that namespace is public.
		/// </summary>
		[EnumMember(Value = "Public")]
		Public = 0,
		/// <summary>
		/// Specifies that namespace is private.
		/// </summary>
		[EnumMember(Value = "Private")]
		Private = 1,
		/// <summary>
		/// Specifies that namespace is public, but read-only.
		/// </summary>
		[EnumMember(Value = "Public Read-Only")]
		PublicReadOnly = 2
	}

	/// <summary>
	/// The class describing namespace.
	/// </summary>
	[DataContract(Name = "namespacedata", Namespace = "")]
	public sealed class Namespace
	{
		/// <summary>
		/// The name of the namespace.
		/// </summary>
		[DataMember(Name = "name", Order = 0)]
		public string Name { get; set; }

		/// <summary>
		/// Number of users in the namespace.
		/// </summary>
		[DataMember(Name = "size", Order = 1)]
		public int Size { get; set; }

		/// <summary>
		/// Share mode of the namespace.
		/// </summary>
		public NamespaceShareMode ShareMode { get; set; }
		[DataMember(Name = "share_mode", Order = 2)]
		private string _ShareMode
		{
			get
			{
				return FCClient.EnumToString(ShareMode);
			}
			set
			{
				ShareMode = FCClient.EnumParse<NamespaceShareMode>(value);
			}
		}

		/// <summary>
		/// Value specifying whether the namespace in owned by the API user.
		/// </summary>
		[DataMember(Name = "owner", Order = 3)]
		public bool IsOwner { get; set; }
	}

	/// <summary>
	/// The class describing saved tag.
	/// </summary>
	[DataContract(Name = "saved_tag", Namespace = "")]
	public sealed class SavedTag
	{
		/// <summary>
		/// Detected tag id.
		/// </summary>
		[DataMember(Name = "detected_tid", EmitDefaultValue = false, Order = 0)]
		public string DetectedTagId { get; set; }

		/// <summary>
		/// Tag id.
		/// </summary>
		[DataMember(Name = "tid", Order = 1)]
		public string TagId { get; set; }
	}

	/// <summary>
	/// The class describing removed tag.
	/// </summary>
	[DataContract(Name = "removed_tag", Namespace = "")]
	public sealed class RemovedTag
	{
		/// <summary>
		/// Removed tag id.
		/// </summary>
		[DataMember(Name = "removed_tid", EmitDefaultValue = false, Order = 0)]
		public string RemovedTagId { get; set; }

		/// <summary>
		/// Tag id.
		/// </summary>
		[DataMember(Name = "tid", Order = 1)]
		public string TagId { get; set; }
	}

	/// <summary>
	/// The group of faces found in photos.
	/// </summary>
	[DataContract(Name = "group", Namespace = "")]
	public sealed class Group
	{
		/// <summary>
		/// A matching user id for the group. Only works if the call provides a list of user ids, and an user id is matched with this group.
		/// </summary>
		[DataMember(Name = "uid", EmitDefaultValue = false, Order = 0)]
		public string UserId { get; set; }

		/// <summary>
		/// The group id.
		/// </summary>
		[DataMember(Name = "gid", EmitDefaultValue = false, Order = 1)]
		public string GroupId { get; set; }

		/// <summary>
		/// A list of one or more tag ids.
		/// </summary>
		/// <remarks>
		/// If the GroupId == <c>null</c> and the UserId == <c>null</c> this means this is the ungrouped group. Those are all the faces that had no matching face, or faces that cannot be group at the moment (such as profile posed faces).
		/// </remarks>
		[DataMember(Name = "tids", Order = 2)]
		public List<string> TagIds { get; set; }
	}

	/// <summary>
	/// The class describing usage of the API.
	/// </summary>
	[DataContract(Name = "usage", Namespace = "")]
	public sealed class Usage
	{
		/// <summary>
		/// Amount of photos that were processed since last rate-limit reset time (1 hour).
		/// </summary>
		[DataMember(Name = "used", Order = 0)]
		public int Used { get; set; }

		/// <summary>
		/// Amount of photos remaining in current time window.
		/// </summary>
		[DataMember(Name = "remaining", Order = 1)]
		public int Remaining { get; set; }

		/// <summary>
		/// The total photos limit permitted in the current 1-hour window.
		/// </summary>
		[DataMember(Name = "limit", Order = 2)]
		public int Limit { get; set; }

		/// <summary>
		/// UTC <see cref="DateTime"/> of next limit reset time.
		/// </summary>
		public DateTime? ResetTime { get; set; }
		[DataMember(Name = "reset_time", Order = 3)]
		private long _ResetTime
		{
			get
			{
				return FCClient.ToUnixTimestamp(ResetTime);
			}
			set
			{
				ResetTime = FCClient.FromUnixTimestamp(value);
			}
		}

		/// <summary>
		/// GMT Unix timestamp of next limit reset time (in text).
		/// </summary>
		[DataMember(Name = "reset_time_text", Order = 0)]
		public string ResetTimeText { get; set; }

		/// <summary>
		/// Amount of namespaces created.
		/// </summary>
		[DataMember(Name = "namespace_used", EmitDefaultValue = false, Order = 1)]
		[DefaultValue(0)]
		public int NamespaceUsed { get; set; }

		/// <summary>
		/// Amount of namespaces remaining (that can be created).
		/// </summary>
		[DataMember(Name = "namespace_remaining", EmitDefaultValue = false, Order = 2)]
		[DefaultValue(0)]
		public int NamespaceRemaining { get; set; }

		/// <summary>
		/// The maximum amount of namespaces that can be created.
		/// </summary>
		[DataMember(Name = "namespace_limit", EmitDefaultValue = false, Order = 3)]
		[DefaultValue(0)]
		public int NamespaceLimit { get; set; }
	}

	/// <summary>
	/// The class describing a photo that was included in a request.
	/// </summary>
	[DataContract(Name = "photo", Namespace = "")]
	public sealed class Photo
	{
		/// <summary>
		/// Photo URL as specified in the request.
		/// </summary>
		[DataMember(Name = "url", EmitDefaultValue = false, Order = 0)]
		public string Url { get; set; }

		/// <summary>
		/// Photo id, can be used as reference instead of URL for follow-up calls to methods that support it.
		/// </summary>
		[DataMember(Name = "pid", EmitDefaultValue = false, Order = 1)]
		public string PhotoId { get; set; }

		/// <summary>
		/// Photo width in pixels.
		/// </summary>
		[DataMember(Name = "width", EmitDefaultValue = false, Order = 2)]
		public uint Width { get; set; }

		/// <summary>
		/// Photo height in pixels.
		/// </summary>
		[DataMember(Name = "height", EmitDefaultValue = false, Order = 3)]
		public uint Height { get; set; }

		/// <summary>
		/// List of zero or more face <see cref="Tag"/> objects found in the photo.
		/// </summary>
		[DataMember(Name = "tags", Order = 4)]
		public List<Tag> Tags { get; set; }

		/// <summary>
		/// Error code, specified only when an error occurs.
		/// </summary>
		[DataMember(Name = "error_code", EmitDefaultValue = false, Order = 5)]
		[DefaultValue(0)]
		public int ErrorCode { get; set; }

		/// <summary>
		/// Error message, specified only when an error occurs.
		/// </summary>
		[DataMember(Name = "error_message", EmitDefaultValue = false, Order = 6)]
		public string ErrorMessage { get; set; }
	}

	/// <summary>
	/// The face object found in a photo.
	/// </summary>
	[DataContract(Name = "tag", Namespace = "")]
	public sealed class Tag
	{
		/// <summary>
		/// The tag id. Tag ids are temporary until used in <see cref="FCClient.TagsApi.BeginSave"/> calls to associate the tag with a specific user id, at which point they become persistent.
		/// </summary>
		[DataMember(Name = "tid", EmitDefaultValue = false, Order = 0)]
		public string TagId { get; set; }

		/// <summary>
		/// Value indicating whether this tag that can be recognized or can be used for training set.
		/// </summary>
		[DataMember(Name = "recognizable", EmitDefaultValue = false, Order = 1)]
		public bool IsRecognizable { get; set; }

		/// <summary>
		/// The recommended confidence threshold from 0-100% to use when deciding which recognition results to display (<see cref="FCClient.FacesApi.BeginRecognize"/> only).
		/// The threshold changes based on the quality of faces found, as well as the size of the user ids list specified when calling <c>FCClient.FacesApi.BeginRecognize</c>.
		/// </summary>
		[DataMember(Name = "threshold", EmitDefaultValue = false, Order = 2)]
		public int? Threshold { get; set; }

		/// <summary>
		/// List of possible matches for the face tag (<see cref="FCClient.FacesApi.BeginRecognize"/> only).
		/// </summary>
		/// <remarks>
		/// Only user ids that were specified during the <c>FCClient.FacesApi.BeginRecognize</c> call (either explicitly or through a list) will be returned.
		/// Refer to the <see cref="Threshold"/> for a recommendation of which confidence scores are high/low for this call. When tags are saved, the confidence score is always 100.
		/// </remarks>
		[DataMember(Name = "uids", Order = 3)]
		public List<Match> Matches { get; set; }

		/// <summary>
		/// The group id.
		/// </summary>
		[DataMember(Name = "gid", EmitDefaultValue = false, Order = 4)]
		public string GroupId { get; set; }

		/// <summary>
		/// Optional text label describing the tag. Must have been previously specified through a <see cref="FCClient.TagsApi.BeginAdd"/> or <see cref="FCClient.TagsApi.BeginSave"/> call.
		/// </summary>
		[DataMember(Name = "label", Order = 5)]
		public string Label { get; set; }

		/// <summary>
		/// Specifies whether a tag has been confirmed or is in a temporary state.
		/// </summary>
		/// <remarks>
		/// Tags are confirmed through calls to <see cref="FCClient.TagsApi.BeginAdd"/> or <see cref="FCClient.TagsApi.BeginSave"/>. Unconfirmed tags are also referred to as temporary, which means that they are not persisted anywhere.
		/// All confirmed tags have the confidence score of 100.
		/// </remarks>
		[DataMember(Name = "confirmed", Order = 6)]
		public bool IsConfirmed { get; set; }

		/// <summary>
		/// Value indicating whether the tag was added through the <see cref="FCClient.TagsApi.BeginAdd"/> call which supports manual addition of otherwise undetected faces.
		/// </summary>
		[DataMember(Name = "manual", Order = 7)]
		public bool IsManual { get; set; }

		/// <summary>
		/// The tagging user id.
		/// </summary>
		[DataMember(Name = "tagger_id", EmitDefaultValue = false, Order = 8)]
		internal string TaggerId { get; set; }

		/// <summary>
		/// Face tag width as 0-100% of photo width.
		/// </summary>
		[DataMember(Name = "width", Order = 9)]
		public float Width { get; set; }

		/// <summary>
		/// Face tag height as 0-100% of photo height.
		/// </summary>
		[DataMember(Name = "height", Order = 10)]
		public float Height { get; set; }

		/// <summary>
		/// Coordinates of tag face's center point.
		/// </summary>
		[DataMember(Name = "center", EmitDefaultValue = false, Order = 11)]
		public Point Center { get; set; }

		/// <summary>
		/// Coordinates of left eye.
		/// </summary>
		[DataMember(Name = "eye_left", EmitDefaultValue = false, Order = 12)]
		public Point EyeLeft { get; set; }

		/// <summary>
		/// Coordinates of right eye.
		/// </summary>
		[DataMember(Name = "eye_right", EmitDefaultValue = false, Order = 13)]
		public Point EyeRight { get; set; }

		/// <summary>
		/// Coordinates of left edge of mouth.
		/// </summary>
		[DataMember(Name = "mouth_left", EmitDefaultValue = false, Order = 14)]
		public Point MouthLeft { get; set; }

		/// <summary>
		/// Coordinates of right edge of mouth.
		/// </summary>
		[DataMember(Name = "mouth_right", EmitDefaultValue = false, Order = 15)]
		public Point MouthRight { get; set; }

		/// <summary>
		/// Coordinates of mouth center.
		/// </summary>
		[DataMember(Name = "mouth_center", EmitDefaultValue = false, Order = 16)]
		public Point MouthCenter { get; set; }

		/// <summary>
		/// Coordinates of nose tip.
		/// </summary>
		[DataMember(Name = "nose", EmitDefaultValue = false, Order = 17)]
		public Point Nose { get; set; }

		/// <summary>
		/// Coordinates of left ear.
		/// </summary>
		[DataMember(Name = "ear_left", EmitDefaultValue = false, Order = 18)]
		public Point EarLeft { get; set; }

		/// <summary>
		/// Coordinates of right ear.
		/// </summary>
		[DataMember(Name = "ear_right", EmitDefaultValue = false, Order = 19)]
		public Point EarRight { get; set; }

		/// <summary>
		/// Coordinates of chin.
		/// </summary>
		[DataMember(Name = "chin", EmitDefaultValue = false, Order = 20)]
		public Point Chin { get; set; }

		/// <summary>
		/// Yaw (facing sideways) angle value as -90 to 90.
		/// </summary>
		[DataMember(Name = "yaw", Order = 21)]
		public int Yaw { get; set; }

		/// <summary>
		/// Roll (face rotation) angle value as -90 to 90.
		/// </summary>
		[DataMember(Name = "roll", Order = 22)]
		public int Roll { get; set; }

		/// <summary>
		/// Pitch (up or down) angle value as -90 to 90.
		/// </summary>
		[DataMember(Name = "pitch", Order = 23)]
		public int Pitch { get; set; }

		/// <summary>
		/// List of detected facial attributes.
		/// </summary>
		[DataMember(Name = "attributes", Order = 24)]
		public TagAttributes Attributes { get; set; }
	}

	/// <summary>
	/// The X and Y coordinates as 0-100% of photo width and height.
	/// </summary>
	[DataContract(Name = "point", Namespace = "")]
	public sealed class Point
	{
		/// <summary>
		/// The X coordinate as 0-100% of photo width.
		/// </summary>
		[DataMember(Name = "x", Order = 0)]
		public float X { get; set; }
		/// <summary>
		/// The Y coordinate as 0-100% of photo height.
		/// </summary>
		[DataMember(Name = "y", Order = 1)]
		public float Y { get; set; }
	}

	/// <summary>
	/// The base class for attribute value and confidence pair.
	/// </summary>
	[DataContract(Name = "confidence", Namespace = "")]
	public abstract class TagAttribute
	{
		/// <summary>
		/// The confidence level of the attribute from 0-100% interval.
		/// </summary>
		[DataMember(Name = "confidence", Order = 0)]
		public int Confidence { get; set; }
	}

	/// <summary>
	/// Boolean attribute value and confidence pair.
	/// </summary>
	[DataContract(Name = "confidence", Namespace = "")]
	public sealed class BooleanAttribute : TagAttribute
	{
		/// <summary>
		/// The value of the attribute.
		/// </summary>
		[DataMember(Name = "value", Order = 0)]
		public bool Value { get; set; }
	}

	/// <summary>
	/// Integer attribute value and confidence pair.
	/// </summary>
	[DataContract(Name = "confidence", Namespace = "")]
	public sealed class IntegerAttribute : TagAttribute
	{
		/// <summary>
		/// The value of the attribute.
		/// </summary>
		[DataMember(Name = "value", Order = 0)]
		public int Value { get; set; }
	}

	/// <summary>
	/// Enumeration attribute value and confidence pair.
	/// </summary>
	[DataContract(Name = "confidence", Namespace = "")]
	public sealed class EnumAttribute<T> : TagAttribute where T: struct
	{
		/// <summary>
		/// The value of the attribute.
		/// </summary>
		public T Value { get; set; }
		[DataMember(Name = "value", Order = 0)]
		private string _Value
		{
			get
			{
				return FCClient.EnumToString(Value);
			}
			set
			{
				Value = FCClient.EnumParse<T>(value);
			}
		}
	}

	/// <summary>
	/// Specifies the gender of the person.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Gender
	{
		/// <summary>
		/// Specifies that the person is male.
		/// </summary>
		[EnumMember(Value = "male")]
		Male,
		/// <summary>
		/// Specifies that the person is female.
		/// </summary>
		[EnumMember(Value = "female")]
		Female
	}

	/// <summary>
	/// Specifies the state of the person's lips.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Lips
	{
		/// <summary>
		/// Specifies that person's lips are sealed.
		/// </summary>
		[EnumMember(Value = "sealed")]
		Sealed,
		/// <summary>
		/// Specifies that person's lips are parted.
		/// </summary>
		[EnumMember(Value = "parted")]
		Parted,
		/// <summary>
		/// Specifies that person's lips are kissing.
		/// </summary>
		[EnumMember(Value = "kissing")]
		Kissing
	}

	/// <summary>
	/// Specifies the expression of the person in the the photo.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Mood
	{
		/// <summary>
		/// Specifies neutral expression.
		/// </summary>
		[EnumMember(Value = "neutral")]
		Neutral,
		/// <summary>
		/// Specifies surprised expression.
		/// </summary>
		[EnumMember(Value = "surprised")]
		Surprised,
		/// <summary>
		/// Specifies happy expression.
		/// </summary>
		[EnumMember(Value = "happy")]
		Happy,
		/// <summary>
		/// Specifies sad expression.
		/// </summary>
		[EnumMember(Value = "sad")]
		Sad,
		/// <summary>
		/// Specifies angry expression.
		/// </summary>
		[EnumMember(Value = "angry")]
		Angry
	}

	/// <summary>
	/// A possible match for the face <see cref="Tag"/>.
	/// </summary>
	[DataContract(Name = "uid", Namespace = "")]
	public sealed class Match
	{
		/// <summary>
		/// The user id match for the face <see cref="Tag"/>.
		/// </summary>
		[DataMember(Name = "uid", Order = 0)]
		public string UserId { get; set; }
		/// <summary>
		/// The score from 0-100% interval.
		/// </summary>
		[DataMember(Name = "confidence", Order = 1)]
		public int Confidence { get; set; }
	}

	/// <summary>
	/// The attributes of a <see cref="Tag"/>.
	/// </summary>
	[DataContract(Name = "attributes", Namespace = "")]
	public sealed class TagAttributes
	{
		/// <summary>
		/// Value indicating whether this <see cref="Tag"/> is face (always set to <c>true</c>) and confidence. Confidence lower than 50% have high probability being false-positives.
		/// </summary>
		[DataMember(Name = "face", EmitDefaultValue = false, Order = 0)]
		public BooleanAttribute Face { get; set; }

		/// <summary>
		/// Male/female value and confidence.
		/// </summary>
		[DataMember(Name = "gender", EmitDefaultValue = false, Order = 1)]
		public EnumAttribute<Gender> Gender { get; set; }

		/// <summary>
		/// Value indicating whether glasses are detected and confidence.
		/// </summary>
		[DataMember(Name = "glasses", EmitDefaultValue = false, Order = 2)]
		public BooleanAttribute Glasses { get; set; }

		/// <summary>
		/// Value indicating whether dark glasses are detected and confidence.
		/// </summary>
		[DataMember(Name = "dark_glasses", EmitDefaultValue = false, Order = 3)]
		public BooleanAttribute DarkGlasses { get; set; }

		/// <summary>
		/// Value indicating whether a person is smiling in a photo and confidence.
		/// </summary>
		[DataMember(Name = "smiling", EmitDefaultValue = false, Order = 4)]
		public BooleanAttribute Smiling { get; set; }

		/// <summary>
		/// Estimated age in years and confidence level.
		/// </summary>
		[DataMember(Name = "age_est", EmitDefaultValue = false, Order = 5)]
		public IntegerAttribute AgeEstimation { get; set; }

		/// <summary>
		/// Minimum age in years and confidence level.
		/// </summary>
		[DataMember(Name = "age_min", EmitDefaultValue = false, Order = 6)]
		public IntegerAttribute AgeMinimum { get; set; }

		/// <summary>
		/// Maximum age in years and confidence level.
		/// </summary>
		[DataMember(Name = "age_max", EmitDefaultValue = false, Order = 7)]
		public IntegerAttribute AgeMaximum { get; set; }

		/// <summary>
		/// Expression of the person in the the photo and confidence.
		/// </summary>
		[DataMember(Name = "mood", EmitDefaultValue = false, Order = 8)]
		public EnumAttribute<Mood> Mood { get; set; }

		/// <summary>
		/// The state of the person's lips and confidence.
		/// </summary>
		[DataMember(Name = "lips", EmitDefaultValue = false, Order = 9)]
		public EnumAttribute<Lips> Lips { get; set; }
	}

	/// <summary>
	/// The training status of the user.
	/// </summary>
	[DataContract(Name = "user_status", Namespace = "")]
	public sealed class UserStatus
	{
		/// <summary>
		/// The user id that was trained.
		/// </summary>
		[DataMember(Name = "uid", Order = 0)]
		public string UserId { get; set; }

		/// <summary>
		/// The number of photos used to train this model.
		/// </summary>
		[DataMember(Name = "training_set_size", Order = 1)]
		public int TrainingSetSize { get; set; }

		/// <summary>
		/// The last UTC <see cref="DateTime"/> when this user id was trained.
		/// </summary>
		public DateTime? LastTrained { get; set; }
		[DataMember(Name = "last_trained", Order = 2)]
		private long _LastTrained
		{
			get
			{
				return FCClient.ToUnixTimestamp(LastTrained);
			}
			set
			{
				LastTrained = FCClient.FromUnixTimestamp(value);
			}
		}

		/// <summary>
		/// Value indicating whether another train is running to the user id at the moment.
		/// </summary>
		[DataMember(Name = "training_in_progress", Order = 3)]
		public bool IsTrainingInProgress { get; set; }
	}
}
