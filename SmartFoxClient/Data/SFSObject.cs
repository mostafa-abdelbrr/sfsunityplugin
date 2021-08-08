using System.Collections;

namespace SmartFoxClientAPI.Data
{
    /**
     * <summary>SFS object class</summary>
     * 
     * <remarks>
     * <para><b>Version:</b><br/>
     * 1.0.0</para>
     * 
     * <para><b>Author:</b><br/>
     * Thomas Hentschel Lund<br/>
     * 			<a href="http://www.fullcontrol.dk">http://www.fullcontrol.dk</a><br/>
     * 			<a href="mailto:sfs-api@fullcontrol.dk">sfs-api@fullcontrol.dk</a><p/>
     * (c) 2008 gotoAndPlay()<br/>
     *          <a href="http://www.smartfoxserver.com">http://www.smartfoxserver.com</a><br/>
     * 			<a href="http://www.gotoandplay.it">http://www.gotoandplay.it</a><br/>
     * </para>
     * </remarks>
     */
    public class SFSObject
    {
        private Hashtable map;

        /**
         * <summary>
         * SFSObject constructor
         * </summary>
         */
        public SFSObject()
        {
            map = new Hashtable();
        }


        //::::::::::::::::::::::::::::::::::::::::::::::::
        // put data
        //::::::::::::::::::::::::::::::::::::::::::::::::

        /**
         * <summary>
         * Put generic object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void Put(object key, object value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put number object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void PutNumber(object key, double value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put bool object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="value">Value to put into SFSObject</param>
         */
        public void PutBool(object key, bool value)
        {
            map.Add(key.ToString(), value);
        }

        /**
         * <summary>
         * Put List object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="collection">Value to put into SFSObject</param>
         */
        public void PutList(object key, IList collection)
        {
            PopulateList(new SFSObject(), key.ToString(), collection);
        }

        /**
         * <summary>
         * Put Dictionary object value into SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         * <param name="collection">Value to put into SFSObject</param>
         */
        public void PutDictionary(object key, IDictionary collection)
        {
            PopulateDictionary(new SFSObject(), key.ToString(), collection);
        }

        private void PopulateList(SFSObject aobj, string key, IList collection)
        {
            int count = 0;

            if (aobj != this)
                Put(key, aobj);

            foreach (object element in collection)
            {
                if (element is IList)
                    aobj.PutList(count, (IList)element);

                else if (element is IDictionary)
                    aobj.PutDictionary(count, (IDictionary)element);

                else
                    aobj.Put(count, element);

                ++count;
            }
        }

        private void PopulateDictionary(SFSObject aobj, string key, IDictionary collection)
        {
            object itemKey = null;
            object itemValue = null;

            if (aobj != this)
                Put(key, aobj);

            foreach (DictionaryEntry element in collection)
            {
                itemKey = element.Key;
                itemValue = element.Value;

                if (itemValue is IList)
                    aobj.PutList(itemKey, (IList)itemValue);

                else if (itemValue is IDictionary)
                    aobj.PutDictionary(itemKey, (IDictionary)itemValue);

                else
                    aobj.Put(itemKey, itemValue);

            }
        }

        //::::::::::::::::::::::::::::::::::::::::::::::::
        // get data
        //::::::::::::::::::::::::::::::::::::::::::::::::

        /**
         * <summary>
         * Get generic object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public object Get(object key)
        {
            return map[key];
        }

        /**
         * <summary>
         * Get string object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public string GetString(object key)
        {
            return (string)map[key];
        }

        /**
         * <summary>
         * Get double object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public double GetNumber(object key)
        {
            return (double)map[key];
        }

        /**
         * <summary>
         * Get bool object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public bool GetBool(object key)
        {
            return (bool)map[key];
        }

        /**
         * <summary>
         * Get SFSObject object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public SFSObject GetObj(object key)
        {
            return (SFSObject)map[key];
        }

        /**
         * <summary>
         * Get SFSObject object value from SFSObject
         * </summary>
         * 
         * <returns>Object value</returns>
         * 
         * <param name="key">Key name to use</param>
         */
        public SFSObject GetObj(int key)
        {
            return (SFSObject)map[key];
        }

        /**
         * <summary>
         * Get number of values in this SFSObject
         * </summary>
         * 
         * <returns>Number of values</returns>
         */
        public int Size()
        {
            return map.Count;
        }

        /**
         * <summary>
         * Get all keys with values in this SFSObject
         * </summary>
         * 
         * <returns>All keys</returns>
         */
        public ICollection Keys()
        {
            return map.Keys;
        }

        /**
         * <summary>
         * Remove object value from SFSObject
         * </summary>
         * 
         * <param name="key">Key name to use</param>
         */
        public void Remove(object key)
        {
            map.Remove(key);
        }

    }
}
