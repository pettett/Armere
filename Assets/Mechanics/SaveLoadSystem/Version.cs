public readonly struct Version : IBinaryVariableSerializer<Version>
{
	//Takes up 4 bytes - same as an integer
	public readonly byte major;
	public readonly byte minor;
	public readonly ushort patch;

	public Version(byte major, byte minor, ushort patch)
	{
		this.major = major;
		this.minor = minor;
		this.patch = patch;
	}

	public readonly override bool Equals(object obj)
	{
		return obj is Version version &&
			   major == version.major &&
			   minor == version.minor &&
			   patch == version.patch;
	}
	public static bool operator ==(Version lhs, Version rhs)
	{
		return lhs.patch == rhs.patch && lhs.minor == rhs.minor && lhs.major == rhs.major;
	}
	public static bool operator !=(Version lhs, Version rhs) => !(lhs == rhs);


	public static bool operator >(Version lhs, Version rhs) => lhs.patch > rhs.patch && lhs.minor >= rhs.minor && lhs.major >= rhs.major;
	public static bool operator <(Version lhs, Version rhs) => lhs.patch < rhs.patch && lhs.minor <= rhs.minor && lhs.major <= rhs.major;

	public readonly override int GetHashCode()
	{
		return major << 24 + minor << 16 + patch;
	}

	public readonly override string ToString()
	{
		return string.Join(".", major, minor, patch);
	}

	public Version Read(in GameDataReader reader) => new Version(reader.ReadByte(), reader.ReadByte(), reader.ReadUShort());

	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(major);
		writer.WritePrimitive(minor);
		writer.WritePrimitive(patch);
	}
}
