using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SkyBiometry.Client.FC
{
	/// <summary>
	/// Specifies result format.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Format
	{
		/// <summary>
		/// Specifies default format (JSON).
		/// </summary>
		[EnumMember(Value = "")]
		Default,
		/// <summary>
		/// Specifies JSON format.
		/// </summary>
		[EnumMember(Value = "json")]
		Json,
		/// <summary>
		/// Specifies XML format.
		/// </summary>
		[EnumMember(Value = "xml")]
		Xml
	}

	/// <summary>
	/// Specifies detector.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum Detector
	{
		/// <summary>
		/// Specifies default detector (normal).
		/// </summary>
		[EnumMember(Value = "")]
		Default,
		/// <summary>
		/// Specifies normal detector.
		/// </summary>
		[EnumMember(Value = "normal")]
		Normal,
		/// <summary>
		/// Specifies aggressive detector.
		/// </summary>
		[EnumMember(Value = "aggressive")]
		Aggressive
	}

	/// <summary>
	/// Specifies attributes to be returned.
	/// </summary>
	[Flags]
	[DataContract(Namespace = "")]
	public enum Attributes
	{
		/// <summary>
		/// Specifies that none attributes shall be returned.
		/// </summary>
		[EnumMember(Value = "none")]
		None = 0,
		/// <summary>
		/// Specifies that default attributes shall be returned (shall not be combined with other values).
		/// </summary>
		[EnumMember(Value = "")]
		Default = 1,
		/// <summary>
		/// Specifies that gender attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "gender")]
		Gender = 2,
		/// <summary>
		/// Specifies that glasses attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "glasses")]
		Glasses = 4,
		/// <summary>
		/// Specifies that smiling attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "smiling")]
		Smiling = 8,
		/// <summary>
		/// Specifies that mood attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "mood")]
		Mood = 16,
		/// <summary>
		/// Specifies that lips attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "lips")]
		Lips = 32,
		/// <summary>
		/// Specifies that age attribute shall be returned.
		/// </summary>
		[EnumMember(Value = "age")]
		Age = 64,
		/// <summary>
		/// Specifies that all attributes shall be returned.
		/// </summary>
		[EnumMember(Value = "all")]
		All = -1
	}

	/// <summary>
	/// Specifies tag order.
	/// </summary>
	[DataContract(Namespace = "")]
	public enum TagOrder
	{
		/// <summary>
		/// Specifies default order (recent).
		/// </summary>
		[EnumMember(Value = "")]
		Default,
		/// <summary>
		/// Specifies most-recent order.
		/// </summary>
		[EnumMember(Value = "recent")]
		Recent,
		/// <summary>
		/// Specifies random order.
		/// </summary>
		[EnumMember(Value = "random")]
		Random
	}

	/// <summary>
	/// Class encapsulating REST SkyBiometry FC API.
	/// </summary>
	public sealed class FCClient
	{
		#region Public types

		/// <summary>
		/// Base class for API.
		/// </summary>
		public abstract class Api
		{
			#region Private fields

			private readonly FCClient _owner;
			private readonly string _name;

			#endregion

			#region Internal constuctor

			internal Api(FCClient owner, string name)
			{
				_owner = owner;
				_name = name;
			}

			#endregion

			#region Protected methods

			/// <summary>
			/// Begins an asynchronous REST method call.
			/// </summary>
			/// <param name="method">The name of the method (without API name).</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			protected IAsyncResult BeginCallMethod(string method, AsyncCallback callback, object state)
			{
				return BeginCallMethod(method, null, callback, state, null);
			}

			/// <summary>
			/// Begins an asynchronous REST method call with the specified arguments.
			/// </summary>
			/// <param name="method">The name of the method (without API name).</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <param name="args">Array of argument name and value pair to be passed to the method.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			protected IAsyncResult BeginCallMethod(string method, AsyncCallback callback, object state, params KeyValuePair<string, object>[] args)
			{
				return BeginCallMethod(method, null, callback, state, args);
			}

			/// <summary>
			/// Begins an asynchronous REST method call with the specified image streams and arguments.
			/// </summary>
			/// <param name="method">The name of the method (without API name).</param>
			/// <param name="imageStreams">An <see cref="Stream"/> list containing images to be passed to the method.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <param name="args">Array of argument name and value pair to be passed to the method.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			protected IAsyncResult BeginCallMethod(string method, IEnumerable<Stream> imageStreams, AsyncCallback callback, object state, params KeyValuePair<string, object>[] args)
			{
				return _owner.BeginCallMethod(_name, method, imageStreams, args, callback, state);
			}

			/// <summary>
			/// Ends an asynchronous REST method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			protected FCResult EndCallMethod(IAsyncResult asyncResult)
			{
				return _owner.EndCallMethod(asyncResult);
			}

			#endregion
		}

		/// <summary>
		/// Class encapsulating SkyBiometry FC account API.
		/// </summary>
		public sealed class AccountApi : Api
		{
			#region Internal constuctor

			internal AccountApi(FCClient owner)
				: base(owner, "account/")
			{
			}

			#endregion

			#region Public methods

			/// <summary>
			/// Begins an asynchronous REST account/authenticate method call. Upon completion returns authentication status.
			/// </summary>
			/// <remarks>
			/// Method can be used to test connection and/or authentication to the API access point. It is not required to call this method before calling any other API methods.
			/// </remarks>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.EndAuthenticate"/>
			public IAsyncResult BeginAuthenticate(AsyncCallback callback, object state)
			{
				return BeginCallMethod("authenticate", callback, state);
			}

			/// <summary>
			/// Ends an asynchronous REST account/authenticate method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.BeginAuthenticate"/>
			public FCResult EndAuthenticate(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST account/users method call. Upon completion returns tags that were registered in the specified user <paramref name="namespaces"/>.
			/// </summary>
			/// <remarks>
			/// Tags are added to namespaces by calling <see cref="FCClient.TagsApi.BeginSave"/> method.
			/// </remarks>
			/// <param name="namespaces">A list of one or more namespaces.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.EndUsers"/>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			public IAsyncResult BeginUsers(IEnumerable<string> @namespaces, AsyncCallback callback, object state)
			{
				return BeginCallMethod("users", callback, state,
					new KeyValuePair<string, object>("namespaces", @namespaces));
			}

			/// <summary>
			/// Ends an asynchronous REST account/users method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.BeginUsers"/>
			public FCResult EndUsers(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST account/limits method call. Upon completion returns limits quota usage information for calling application.
			/// </summary>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.EndLimits"/>
			public IAsyncResult BeginLimits(AsyncCallback callback, object state)
			{
				return BeginCallMethod("limits", callback, state);
			}

			/// <summary>
			/// Ends an asynchronous REST account/limits method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.BeginLimits"/>
			public FCResult EndLimits(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST account/namespaces method call. Upon completion returns all valid namespaces for user authorized by apiKey specified in <see cref="FCClient"/> constructor.
			/// </summary>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.EndNamespaces"/>
			public IAsyncResult BeginNamespaces(AsyncCallback callback, object state)
			{
				return BeginCallMethod("namespaces", callback, state);
			}

			/// <summary>
			/// Ends an asynchronous REST account/namespaces method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.AccountApi.BeginNamespaces"/>
			public FCResult EndNamespaces(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			#endregion
		}

		/// <summary>
		/// Class encapsulating SkyBiometry FC faces API.
		/// </summary>
		public sealed class FacesApi : Api
		{
			#region Internal constuctor

			internal FacesApi(FCClient owner)
				: base(owner, "faces/")
			{
			}

			#endregion

			#region Public methods

			/// <summary>
			/// Begins an asynchronous REST faces/detect method call. Upon completion returns tags for detected faces in one or more photos.
			/// </summary>
			/// <remarks>
			/// <para>Detected faces will contain geometric information of the tag, eyes, nose and mouth, as well as additional attributes such as gender.</para>
			/// <para>Method call usage is limited. Each passed image (either <paramref name="urls"/> or through <paramref name="imageStreams"/>) is added to your allowed usage.</para>
			/// </remarks>
			/// <param name="urls">A list of of image URLs.</param>
			/// <param name="imageStreams">A <see cref="Stream"/> list containing images to be passed to the method.</param>
			/// <param name="detector">Face detector to use. <see cref="Detector.Normal"/> (<see cref="Detector.Default"/>) – fast face and attribute detection, <see cref="Detector.Aggressive"/> – more accurate and slower face and attribute detection.</param>
			/// <param name="attributes">Specifies which attributes will be returned with the results. Can be <see cref="Attributes.Default"/>, <see cref="Attributes.None"/>, <see cref="Attributes.All"/> or a bitwise combination of one or more other <see cref="Attributes"/> values.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.EndDetect"/>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			public IAsyncResult BeginDetect(IEnumerable<string> urls, IEnumerable<Stream> imageStreams, Detector detector, Attributes attributes, AsyncCallback callback, object state)
			{
				return BeginCallMethod("detect", imageStreams, callback, state,
					new KeyValuePair<string, object>("urls", urls),
					new KeyValuePair<string, object>("detector", detector),
					new KeyValuePair<string, object>("attributes", attributes));
			}

			/// <summary>
			/// Ends an asynchronous REST faces/detect method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			public FCResult EndDetect(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST faces/recognize method call. Method is used for recognizing trained user ids in one or more photos.
			/// </summary>
			/// <remarks>
			/// <para>Upon completion for each detected face, method will return user ids that match specified face or empty result set if no matches found. Each tag also includes a threshold score, if matching score is below this threshold - matched user id can be treated as unlikely match.</para>
			/// <para>Method call usage is limited. Each passed image (either <paramref name="urls"/> or through <paramref name="imageStreams"/>) is added to your allowed usage.</para>
			/// </remarks>
			/// <param name="userIds">A list of user ids to search for.</param>
			/// <param name="urls">A list of of image URLs.</param>
			/// <param name="imageStreams">A <see cref="Stream"/> list containing images to be passed to the method.</param>
			/// <param name="namespace">Default namespace to be used for all specified user ids without namespace specified. Optional, can be null.</param>
			/// <param name="detector">Face detector to use. <see cref="Detector.Normal"/> (<see cref="Detector.Default"/>) – fast face and attribute detection, <see cref="Detector.Aggressive"/> – more accurate and slower face and attribute detection.</param>
			/// <param name="attributes">Specifies which attributes will be returned with the results. Can be <see cref="Attributes.Default"/>, <see cref="Attributes.None"/>, <see cref="Attributes.All"/> or a bitwise combination of one or more other <see cref="Attributes"/> values.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.EndRecognize"/>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			public IAsyncResult BeginRecognize(IEnumerable<string> userIds, IEnumerable<string> urls, IEnumerable<Stream> imageStreams, string @namespace, Detector detector, Attributes attributes, AsyncCallback callback, object state)
			{
				return BeginCallMethod("recognize", imageStreams, callback, state,
					new KeyValuePair<string, object>("uids", userIds),
					new KeyValuePair<string, object>("urls", urls),
					new KeyValuePair<string, object>("namespace", @namespace),
					new KeyValuePair<string, object>("detector", detector),
					new KeyValuePair<string, object>("attributes", attributes));
			}

			/// <summary>
			/// Ends an asynchronous REST faces/recognize method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			public FCResult EndRecognize(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST faces/train method call. Method is used to train specified user ids.
			/// </summary>
			/// <remarks>
			/// <para>Method uses tags previously saved using <see cref="FCClient.TagsApi.BeginSave"/> method and creates face templates for the specified user ids.</para>
			/// <para>Once the face tags were trained, specified user id can be recognized using <see cref="FCClient.FacesApi.BeginRecognize"/> method calls.</para>
			/// </remarks>
			/// <param name="userIds">A list of user ids to begin training for.</param>
			/// <param name="namespace">Default namespace to be used for all specified user ids without namespace specified. Optional, can be null.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.EndTrain"/>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			public IAsyncResult BeginTrain(IEnumerable<string> userIds, string @namespace, AsyncCallback callback, object state)
			{
				return BeginCallMethod("train", callback, state,
					new KeyValuePair<string, object>("uids", userIds),
					new KeyValuePair<string, object>("namespace", @namespace));
			}

			/// <summary>
			/// Ends an asynchronous REST faces/train method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			public FCResult EndTrain(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST faces/group method call. Method can be used to detect, group and optionally recognize one or more user faces in one or more photos.
			/// </summary>
			/// <remarks>
			/// <para>
			///   The method tries to match all the faces that were found in the images specified by <paramref name="urls"/> or through <paramref name="imageStreams"/> one to other,
			///   then assigns a group id for all detected faces that appear to be of the same person.
			///   If user ids are specified when calling this methods, method also attempts to assign the most likely user id for each detected face/group of faces.
			///   Returned result are similar to <see cref="FCClient.FacesApi.BeginRecognize"/> method results.
			/// </para>
			/// <para>Method call usage is limited. Each passed image (either <paramref name="urls"/> or through <paramref name="imageStreams"/>) is added to your allowed usage.</para>
			/// </remarks>
			/// <param name="userIds">A list of user ids to search for.</param>
			/// <param name="urls">A list of of image URLs.</param>
			/// <param name="imageStreams">A <see cref="Stream"/> list containing images to be passed to the method.</param>
			/// <param name="namespace">Default namespace to be used for all specified user ids without namespace specified. Optional, can be null.</param>
			/// <param name="detector">Face detector to use. <see cref="Detector.Normal"/> (<see cref="Detector.Default"/>) – fast face and attribute detection, <see cref="Detector.Aggressive"/> – more accurate and slower face and attribute detection.</param>
			/// <param name="attributes">Specifies which attributes will be returned with the results. Can be <see cref="Attributes.Default"/>, <see cref="Attributes.None"/>, <see cref="Attributes.All"/> or a bitwise combination of one or more other <see cref="Attributes"/> values.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.EndGroup"/>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			public IAsyncResult BeginGroup(IEnumerable<string> userIds, IEnumerable<string> urls, IEnumerable<Stream> imageStreams, string @namespace, Detector detector, Attributes attributes, AsyncCallback callback, object state)
			{
				return BeginCallMethod("group", imageStreams, callback, state,
					new KeyValuePair<string, object>("uids", userIds),
					new KeyValuePair<string, object>("urls", urls),
					new KeyValuePair<string, object>("namespace", @namespace),
					new KeyValuePair<string, object>("detector", detector),
					new KeyValuePair<string, object>("attributes", attributes));
			}

			/// <summary>
			/// Ends an asynchronous REST faces/group method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.BeginGroup"/>
			public FCResult EndGroup(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST faces/group method call. Upon completion returns training status for specified user ids.
			/// </summary>
			/// <param name="userIds">A list of user ids to get training information for.</param>
			/// <param name="namespace">Default namespace to be used for all specified user ids without namespace specified. Optional, can be null.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.EndStatus"/>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			public IAsyncResult BeginStatus(IEnumerable<string> userIds, string @namespace, AsyncCallback callback, object state)
			{
				return BeginCallMethod("status", callback, state,
					new KeyValuePair<string, object>("uids", userIds),
					new KeyValuePair<string, object>("namespace", @namespace));
			}

			/// <summary>
			/// Ends an asynchronous REST faces/status method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.FacesApi.BeginStatus"/>
			public FCResult EndStatus(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			#endregion
		}

		/// <summary>
		/// Class encapsulating SkyBiometry FC tags API.
		/// </summary>
		public sealed class TagsApi : Api
		{
			#region Internal constuctor

			internal TagsApi(FCClient owner)
				: base(owner, "tags/")
			{
			}

			#endregion

			#region Public methods

			/// <summary>
			/// Begins an asynchronous REST tags/add method call to add face tags manually.
			/// </summary>
			/// <remarks>
			/// Tags added manually can not be used to enroll/train user to namespace, this is the only difference between the automatic tags (obtained through <see cref="FCClient.FacesApi.BeginDetect"/> method) and manual.
			/// </remarks>
			/// <param name="url">URL of the image to add the tag to.</param>
			/// <param name="x">Horizontal center position of the tag, as a percentage from 0 to 100, from the left of the photo.</param>
			/// <param name="y">Vertical center position of the tag, as a percentage from 0 to 100, from the left of the photo.</param>
			/// <param name="width">Width of the tag, as a percentage from 0 to 100.</param>
			/// <param name="height">Height of the tag, as a percentage from 0 to 100.</param>
			/// <param name="userId">Id of the user being tagged.</param>
			/// <param name="label">Display name of the user being tagged (e.g. First and Last name). Optional, can be null.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.EndAdd"/>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			public IAsyncResult BeginAdd(string url, float x, float y, float width, float height, string userId, string label, AsyncCallback callback, object state)
			{
				return BeginCallMethod("add", callback, state,
					new KeyValuePair<string, object>("url", url),
					new KeyValuePair<string, object>("x", x),
					new KeyValuePair<string, object>("y", y),
					new KeyValuePair<string, object>("width", width),
					new KeyValuePair<string, object>("height", height),
					new KeyValuePair<string, object>("uid", userId),
					new KeyValuePair<string, object>("label", label));
			}

			/// <summary>
			/// Ends an asynchronous REST tags/add method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.BeginAdd"/>
			public FCResult EndAdd(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST tags/save method call to save the specified face tags to permanent storage.
			/// </summary>
			/// <remarks>
			/// Once the face tag has been saved, you can call <see cref="FCClient.FacesApi.BeginTrain"/> method,
			/// which will use the saved tag information to create face template for specified user id and will add it to specified namespace.
			/// When completed you can start recognizing the specified user id (using <see cref="FCClient.FacesApi.BeginRecognize"/> method).
			/// </remarks>
			/// <param name="tagIds">One or more tag ids to associate with the specified <paramref name="userId"/>. Tag id is a reference field in the response of <see cref="FCClient.FacesApi.BeginDetect"/> and <see cref="FCClient.FacesApi.BeginRecognize"/> methods.</param>
			/// <param name="userId">Id of the user being tagged (e.g. mark@docs, where mark – is the name of your choice and docs is the name of created namespace).</param>
			/// <param name="label">Display name of the user being tagged (e.g. First and Last name). Optional, can be null.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.EndSave"/>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			/// <seealso cref="FCClient.TagsApi.BeginAdd"/>
			public IAsyncResult BeginSave(IEnumerable<string> tagIds, string userId, string label, AsyncCallback callback, object state)
			{
				return BeginCallMethod("save", callback, state,
					new KeyValuePair<string, object>("tids", tagIds),
					new KeyValuePair<string, object>("uid", userId),
					new KeyValuePair<string, object>("label", label));
			}

			/// <summary>
			/// Ends an asynchronous REST tags/save method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			public FCResult EndSave(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST tags/remove method call to remove the previously saved tag(s) using <see cref="FCClient.TagsApi.BeginSave"/>.
			/// </summary>
			/// <remarks>
			/// When removing tag that was trained to namespace using <see cref="FCClient.FacesApi.BeginRecognize"/>,
			/// you must call <see cref="FCClient.FacesApi.BeginRecognize"/> again to persist changes made by removing specified tag.
			/// </remarks>
			/// <param name="tagIds">One or more tag ids to remove. Tag id is a reference field in the response of <see cref="FCClient.FacesApi.BeginDetect"/>, <see cref="FCClient.FacesApi.BeginRecognize"/> and <see cref="FCClient.TagsApi.BeginGet"/> methods.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.EndRemove"/>
			/// <seealso cref="FCClient.FacesApi.BeginTrain"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			/// <seealso cref="FCClient.TagsApi.BeginGet"/>
			public IAsyncResult BeginRemove(IEnumerable<string> tagIds, AsyncCallback callback, object state)
			{
				return BeginCallMethod("remove", callback, state,
					new KeyValuePair<string, object>("tids", tagIds));
			}

			/// <summary>
			/// Ends an asynchronous REST tags/remove method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.BeginRemove"/>
			public FCResult EndRemove(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			/// <summary>
			/// Begins an asynchronous REST tags/get method call to get already saved tags to namespace. By specifying different parameters and criteria you can influence the returned upon completion tags.
			/// </summary>
			/// <param name="userIds">A list of user ids to get tags for.</param>
			/// <param name="urls">A list of of image URLs.</param>
			/// <param name="imageStreams">A <see cref="Stream"/> list containing images to be passed to the method.</param>
			/// <param name="photoIds">A list of photo ids to get tags for (photo ids are returned for <see cref="FCClient.FacesApi.BeginDetect"/> and <see cref="FCClient.FacesApi.BeginRecognize"/>).</param>
			/// <param name="order">Specifies the order of returned tags.</param>
			/// <param name="limit">Maximum number of tags to return (default is 5). Optional, can be null.</param>
			/// <param name="together">If <c>true</c> then when multiple <paramref name="userIds"/> are provided, return only tags for photos where all user ids appear together in the photo(s) (default is false).</param>
			/// <param name="filter">Ability to specify facial attributes for filtering the returned tags. Optional, can be null.</param>
			/// <param name="namespace">Default namespace to be used for all specified user ids without namespace specified.</param>
			/// <param name="callback">The <see cref="AsyncCallback"/> delegate to be called when the asynchronous method call is completed.</param>
			/// <param name="state">The state object for this method call that will be passed to <paramref name="callback"/>.</param>
			/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request for the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.EndGet"/>
			/// <seealso cref="FCClient.FacesApi.BeginDetect"/>
			/// <seealso cref="FCClient.FacesApi.BeginRecognize"/>
			/// <seealso cref="FCClient.TagsApi.BeginSave"/>
			/// <seealso cref="FCClient.TagsApi.BeginAdd"/>
			public IAsyncResult BeginGet(IEnumerable<string> userIds, IEnumerable<string> urls, IEnumerable<Stream> imageStreams, IEnumerable<string> photoIds, TagOrder order, int? limit, bool? together, string filter, string @namespace, AsyncCallback callback, object state)
			{
				return BeginCallMethod("get", imageStreams, callback, state,
					new KeyValuePair<string, object>("uids", userIds),
					new KeyValuePair<string, object>("urls", urls),
					new KeyValuePair<string, object>("pids", photoIds),
					new KeyValuePair<string, object>("order", order),
					new KeyValuePair<string, object>("limit", limit),
					new KeyValuePair<string, object>("together", together),
					new KeyValuePair<string, object>("filter", filter),
					new KeyValuePair<string, object>("namespace", @namespace));
			}

			/// <summary>
			/// Ends an asynchronous REST tags/get method call.
			/// </summary>
			/// <param name="asyncResult">The pending asynchronous request to wait for.</param>
			/// <returns>A <see cref="FCResult"/> than contains result of the method call.</returns>
			/// <seealso cref="FCClient.TagsApi.BeginGet"/>
			public FCResult EndGet(IAsyncResult asyncResult)
			{
				return EndCallMethod(asyncResult);
			}

			#endregion
		}

		#endregion

		#region Private constants

		private const long UnixEpoch = 621355968000000000L;
		private const string DefaultServer = "http://api.skybiometry.com/fc/";

		#endregion

		#region Internal static methods

		internal static DateTime? FromUnixTimestamp(long value)
		{
			return value <= 0 ? (DateTime?)null : new DateTime(checked(UnixEpoch + value * TimeSpan.TicksPerSecond), DateTimeKind.Utc);
		}

		internal static long ToUnixTimestamp(DateTime? value)
		{
			return value.HasValue ? checked((value.Value.Ticks - UnixEpoch) / TimeSpan.TicksPerSecond) : 0;
		}

		private static string GetEnumFieldName(FieldInfo value)
		{
			if (value == null) throw new ArgumentNullException("value");
			var attrs = (EnumMemberAttribute[])value.GetCustomAttributes(typeof(EnumMemberAttribute), false);
			return attrs.Length != 0 ? attrs[0].Value : value.Name;
		}

		internal static string EnumToString(Type enumType, object value)
		{
			if (enumType == null) throw new ArgumentNullException("enumType");
			if (!enumType.IsEnum) throw new ArgumentException("enumType is not an Enum");
			bool isFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0;
			List<string> names = null;
			int v = Convert.ToInt32(value);
			foreach (var fi in enumType.GetFields())
			{
				if (!fi.IsStatic) continue;
				int f = Convert.ToInt32(fi.GetValue(null));
				if (v == f)
				{
					return GetEnumFieldName(fi);
				}
				if (isFlags && f != 0 && (v & f) == f)
				{
					if (names == null) names = new List<string>();
					names.Add(GetEnumFieldName(fi));
				}
			}
			if (names == null) throw new ArgumentException("value has no match in enumType", "value");
			return string.Join(",", names);
		}

		internal static string EnumToString<T>(T value) where T : struct
		{
			return EnumToString(typeof(T), value);
		}

		internal static object EnumParse(Type enumType, string value)
		{
			if (enumType == null) throw new ArgumentNullException("enumType");
			if (!enumType.IsEnum) throw new ArgumentException("enumType is not an Enum");
			bool isFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0;
			string[] values = value != null && isFlags ? value.Split(',') : new [] { value ?? string.Empty };
			var fieldInfos = enumType.GetFields();
			int v = 0;
			foreach (var val in values)
			{
				bool found = false;
				foreach (var fi in fieldInfos)
				{
					if (!fi.IsStatic) continue;
					if (string.CompareOrdinal(val, GetEnumFieldName(fi)) == 0)
					{
						object f = fi.GetValue(null);
						if (!isFlags) return f;
						v |= Convert.ToInt32(f);
						found = true;
						break;
					}
				}
				if (!found) throw new FormatException("value has no match in enumType");
			}
			return Enum.ToObject(enumType, v);
		}

		internal static T EnumParse<T>(string value) where T : struct
		{
			return (T)EnumParse(typeof(T), value);
		}

		#endregion

		#region Private fields

		private readonly string _apiKey;
		private readonly string _apiSecret;
		private readonly string _password;
		private readonly Format _format;
		private readonly string _formatString;
		private readonly bool _getRawData;
		private string _server = "http://api.skybiometry.com/fc/";

		#endregion

		#region Public fields

		/// <summary>
		/// Account API object.
		/// </summary>
		public readonly AccountApi Account;
		/// <summary>
		/// Faces API object.
		/// </summary>
		public readonly FacesApi Faces;
		/// <summary>
		/// Tags API object.
		/// </summary>
		public readonly TagsApi Tags;

		#endregion

		#region Public constructors

		/// <summary>
		/// Initializes new instance of FCClient class with the specified connection parameters.
		/// </summary>
		/// <param name="apiKey">SkyBiometry API key.</param>
		/// <param name="apiSecret">SkyBiometry API secret.</param>
		/// <param name="password">SkyBiometry API password for tag operations (optional).</param>
		public FCClient(string apiKey, string apiSecret, string password = null)
			: this(apiKey, apiSecret, password, Format.Default, false)
		{
		}

		/// <summary>
		/// Initializes new instance of FCClient class with the specified connection parameters, result format and whether to retrieve raw response data.
		/// </summary>
		/// <param name="apiKey">SkyBiometry API key.</param>
		/// <param name="apiSecret">SkyBiometry API secret.</param>
		/// <param name="password">SkyBiometry API password for tag operations (optional).</param>
		/// <param name="format">Result format.</param>
		/// <param name="getRawData">Value specifying whether to retrieve raw response data.</param>
		public FCClient(string apiKey, string apiSecret, string password, Format format, bool getRawData)
		{
			_apiKey = apiKey;
			_apiSecret = apiSecret;
			_password = password;
			_format = format == Format.Default ? Format.Json : format;
			_formatString = EnumToString(format);
			if (_formatString != string.Empty) _formatString = '.' + _formatString;
			_getRawData = getRawData;
			Account = new AccountApi(this);
			Faces = new FacesApi(this);
			Tags = new TagsApi(this);
		}

		#endregion

		#region Private methods

		private IAsyncResult BeginCallMethod(string name, string method, IEnumerable<Stream> imageStreams, IEnumerable<KeyValuePair<string, object>> args, AsyncCallback callback, object state)
		{
			string url = string.Concat(Server, name, method, _formatString);
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			string boundary = null;
			bool useForm = imageStreams != null;
			var theArgs = new List<KeyValuePair<string, string>>();
			foreach (var arg in PrepareArgs(args))
			{
				theArgs.Add(arg);
				if (!useForm && arg.Value.IndexOf('&') != -1) useForm = true;
			}
			if (useForm)
			{
				boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
				request.ContentType = "multipart/form-data; boundary=" + boundary;

			}
			else
			{
				request.ContentType = "application/x-www-form-urlencoded";
			}
			var asyncResult = new AsyncResult<FCResult>(callback, state);
			request.BeginGetRequestStream(grsAsyncResult =>
				{
					try
					{
						using (var requestStream = request.EndGetRequestStream(grsAsyncResult))
						using (var writer = new StreamWriter(requestStream))
						{
							if (useForm)
							{
								foreach (var arg in theArgs)
								{
									writer.Write("\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}", arg.Key, arg.Value);
								}
								if (imageStreams != null)
								{
									foreach (var imageStream in imageStreams)
									{
										writer.Write("\r\n--" + boundary + "\r\n");
										writer.Write("Content-Disposition: form-data; name=\"_file\"; filename=\"_files\"\r\n Content-Type: application/octet-stream\r\n\r\n");
										writer.Flush();
										imageStream.CopyTo(requestStream);
									}
								}

							}
							else
							{
								int i = 0;
								foreach (var arg in theArgs)
								{
									if (i++ != 0) writer.Write('&');
									writer.Write(arg.Key);
									writer.Write('=');
									writer.Write(arg.Value);
								}
							}
						}
						request.BeginGetResponse(GetResponseCallback, new KeyValuePair<HttpWebRequest, AsyncResult<FCResult>>(request, ((AsyncResult<FCResult>)grsAsyncResult.AsyncState)));
					}
					catch (Exception e)
					{
						((AsyncResult<FCResult>)grsAsyncResult.AsyncState).SetAsCompleted(e, false);
					}
				}, asyncResult);
			return asyncResult;
		}

		private FCResult EndCallMethod(IAsyncResult asyncResult)
		{
			return ((AsyncResult<FCResult>)asyncResult).EndInvoke();
		}

		private IEnumerable<KeyValuePair<string, string>> PrepareArgs(IEnumerable<KeyValuePair<string, object>> args)
		{
			if (!string.IsNullOrWhiteSpace(_apiKey)) yield return new KeyValuePair<string, string>("api_key", _apiKey);
			if (!string.IsNullOrWhiteSpace(_apiSecret)) yield return new KeyValuePair<string, string>("api_secret", _apiSecret);
			if (!string.IsNullOrWhiteSpace(_password)) yield return new KeyValuePair<string, string>("password", _password);
			if (args != null)
			{
				foreach (var arg in args)
				{
					if (arg.Value != null)
					{
						var s = arg.Value as string;
						if (s == null)
						{
							var sl = arg.Value as IEnumerable<string>;
							if (sl != null)
							{
								s = string.Join(",", sl);
							}
							else if (arg.Value is Enum)
							{
								s = EnumToString(arg.Value.GetType(), arg.Value);
							}
							else
							{
								s = Convert.ToString(arg.Value, CultureInfo.InvariantCulture);
							}
						}
						if (!string.IsNullOrWhiteSpace(s)) yield return new KeyValuePair<string, string>(arg.Key, s);
					}
				}
			}
		}

		private FCResult ParseResponse(Stream responseStream)
		{
			StreamReader reader = null;
			try
			{
				string rawData;
				if (_getRawData)
				{
					var readStream = new MemoryStream();
					responseStream.CopyTo(readStream);
					readStream.Position = 0;
					reader = new StreamReader(readStream, false);
					rawData = reader.ReadToEnd();
					readStream.Position = 0;
					responseStream.Dispose(); responseStream = readStream;
				}
				else rawData = null;
				FCResult result;
				switch (_format)
				{
					case Format.Json:
						{
							var serializer = new DataContractJsonSerializer(typeof(FCResult));
							result = (FCResult)serializer.ReadObject(responseStream);
						}
						break;
					case Format.Xml:
						{
							var serializer = new DataContractSerializer(typeof(FCResult));
							result = (FCResult)serializer.ReadObject(responseStream);
						}
						break;
					default: throw new NotImplementedException();
				}
				result.RawData = rawData;
				if (reader != null) reader.Dispose();
				return result;
			}
			finally
			{
				responseStream.Dispose();
			}
		}

		private void GetResponseCallback(IAsyncResult asyncResult)
		{
			var asyncState = (KeyValuePair<HttpWebRequest, AsyncResult<FCResult>>)asyncResult.AsyncState;
			HttpWebRequest request = asyncState.Key;
			AsyncResult<FCResult> theAsyncResult = asyncState.Value;
			try
			{
				using (var response = (HttpWebResponse)request.EndGetResponse(asyncResult))
				{
					using (var responseStream = response.GetResponseStream())
					{
						theAsyncResult.SetAsCompleted(ParseResponse(responseStream), false);
					}
				}
			}
			catch (WebException ex)
			{
				var httpResponse = ex.Response as HttpWebResponse;
				if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.BadRequest) throw;
				try
				{
					using (var response = httpResponse)
					{
						using (var responseStream = response.GetResponseStream())
						{
							theAsyncResult.SetAsCompleted(ParseResponse(responseStream), false);
						}
					}
				}
				catch (Exception e)
				{
					theAsyncResult.SetAsCompleted(e, false);
				}
			}
			catch (Exception e)
			{
				theAsyncResult.SetAsCompleted(e, false);
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets server address. This property can be useful for enabling secure https:// connection.
		/// Default value is http://api.skybiometry.com/fc/.
		/// </summary>
		public string Server
		{
			get
			{
				return _server;
			}
			set
			{
				_server = value;
			}
		}

		#endregion
	}
}
